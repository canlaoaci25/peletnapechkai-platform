using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Automation;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static partial class AutomationWorkerEndpoints
{
    private static async Task<IResult> GetCandidatesAsync(Guid id, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        if (job.Status != AutomationJobStatus.Running) return Results.Conflict(new { message = "İş çalışır durumda değil." });

        if (job.Type == AutomationJobType.ContentTranslation)
        {
            var sources = await database.ArticleLocalizations.AsNoTracking()
                .Where(article => article.Status == PublicationStatus.Published && article.Locale.IsDefault)
                .OrderBy(article => article.Id)
                .Select(article => new { article.Id, article.ArticleGroupId, article.Slug, article.Title, article.Summary, article.Body })
                .ToListAsync(token);
            var locales = await database.Locales.AsNoTracking().Where(locale => job.TargetLocales.Contains(locale.Code)).OrderBy(locale => locale.Code).Select(locale => locale.Code).ToListAsync(token);
            var groupIds = sources.Select(source => source.ArticleGroupId).ToArray();
            var existing = await database.ArticleLocalizations.AsNoTracking().Where(article => groupIds.Contains(article.ArticleGroupId) && job.TargetLocales.Contains(article.Locale.Code) && article.Status != PublicationStatus.Archived)
                .Select(article => new { article.ArticleGroupId, Locale = article.Locale.Code }).ToListAsync(token);
            var existingKeys = existing.Select(item => $"{item.ArticleGroupId:N}|{item.Locale}").ToHashSet(StringComparer.OrdinalIgnoreCase);
            var candidates = (from source in sources from locale in locales where !existingKeys.Contains($"{source.ArticleGroupId:N}|{locale}") select new { sourceArticleId = source.Id, sourceArticleGroupId = source.ArticleGroupId, source.Slug, source.Title, source.Summary, source.Body, locale }).Take(5).ToArray();
            return Results.Ok(new { kind = "translation", candidates });
        }

        if (job.Type == AutomationJobType.SeoLocalization)
        {
            var candidates = await database.ArticleLocalizations.AsNoTracking().Where(article => job.TargetLocales.Contains(article.Locale.Code) && article.Status == PublicationStatus.Draft && (article.SeoTitle == null || article.SeoDescription == null))
                .OrderBy(article => article.Id).Take(10).Select(article => new { article.Id, locale = article.Locale.Code, article.Slug, article.Title, article.Summary, article.Body }).ToArrayAsync(token);
            return Results.Ok(new { kind = "seo", candidates });
        }

        return Results.Conflict(new { message = "Bu iş türü yapılandırılmış içerik adayı sağlamaz." });
    }

    private static async Task<IResult> SaveTranslationsAsync(Guid id, EncodedPayload request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        if (job.Type != AutomationJobType.ContentTranslation || job.Status != AutomationJobStatus.Running) return Results.Conflict();
        var payload = DecodePayload<TranslationBatch>(request.PayloadBase64);
        if (payload?.Items is not { Length: > 0 and <= 5 }) return Results.BadRequest(new { message = "Geçersiz çeviri paketi." });
        var now = DateTimeOffset.UtcNow;
        foreach (var item in payload.Items)
        {
            if (!job.TargetLocales.Contains(item.Locale, StringComparer.OrdinalIgnoreCase) || !ValidTranslation(item)) return Results.BadRequest(new { message = "Çeviri alanları geçersiz." });
            var source = await database.ArticleLocalizations.Include(article => article.ArticleGroup).SingleOrDefaultAsync(article => article.Id == item.SourceArticleId && article.Locale.IsDefault && article.Status == PublicationStatus.Published, token);
            var locale = await database.Locales.SingleOrDefaultAsync(candidate => candidate.Code == item.Locale && candidate.IsEnabled, token);
            if (source is null || locale is null) return Results.BadRequest(new { message = "Kaynak veya hedef dil bulunamadı." });
            var exists = await database.ArticleLocalizations.AnyAsync(article => article.ArticleGroupId == source.ArticleGroupId && article.LocaleId == locale.Id && article.Status != PublicationStatus.Archived, token);
            if (exists) continue;
            var slug = item.Slug;
            if (await database.ArticleLocalizations.AnyAsync(article => article.LocaleId == locale.Id && article.Slug == slug, token))
                slug = $"{slug[..Math.Min(slug.Length, 230)]}-{source.ArticleGroupId.ToString("N")[..8]}";
            var article = new ArticleLocalization(source.ArticleGroup, locale, slug, item.Title, item.Summary, SanitizeBody(item.Body), now);
            database.ArticleLocalizations.Add(article);
            database.AuditLogs.Add(new AuditLog(job.CreatedByUserId, "automation.translation_draft_created", nameof(ArticleLocalization), article.Id, JsonSerializer.Serialize(new { sourceArticleId = source.Id, item.Locale, jobId = job.Id }), now));
        }
        await database.SaveChangesAsync(token);
        await UpdateProgressAsync(job, database, token);
        return Results.Ok();
    }

    private static async Task<IResult> SaveSeoDraftsAsync(Guid id, EncodedPayload request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        if (job.Type != AutomationJobType.SeoLocalization || job.Status != AutomationJobStatus.Running) return Results.Conflict();
        var payload = DecodePayload<SeoBatch>(request.PayloadBase64);
        if (payload?.Items is not { Length: > 0 and <= 10 }) return Results.BadRequest(new { message = "Geçersiz SEO paketi." });
        var now = DateTimeOffset.UtcNow;
        foreach (var item in payload.Items)
        {
            if (string.IsNullOrWhiteSpace(item.SeoTitle) || item.SeoTitle.Length > 180 || string.IsNullOrWhiteSpace(item.SeoDescription) || item.SeoDescription.Length > 320) return Results.BadRequest(new { message = "SEO alanları geçersiz." });
            var article = await database.ArticleLocalizations.SingleOrDefaultAsync(candidate => candidate.Id == item.ArticleId && job.TargetLocales.Contains(candidate.Locale.Code), token);
            if (article is null || article.Status != PublicationStatus.Draft) return Results.BadRequest(new { message = "SEO hedef taslağı bulunamadı." });
            article.UpdateDraft(article.Slug, article.Title, article.Summary, article.Body, item.SeoTitle, item.SeoDescription, now);
            database.AuditLogs.Add(new AuditLog(job.CreatedByUserId, "automation.seo_draft_created", nameof(ArticleLocalization), article.Id, JsonSerializer.Serialize(new { job.Id }), now));
        }
        await database.SaveChangesAsync(token);
        await UpdateProgressAsync(job, database, token);
        return Results.Ok();
    }

    private static async Task UpdateProgressAsync(AutomationJob job, PublishingDbContext database, CancellationToken token)
    {
        var remaining = job.Type == AutomationJobType.ContentTranslation
            ? await AutomationCandidateCounter.CountMissingTranslationsAsync(database, job.TargetLocales, token)
            : await AutomationCandidateCounter.CountSeoCandidatesAsync(database, job.TargetLocales, token);
        job.ReportProgress(Math.Max(0, job.TotalItems - remaining), 0, job.CurrentPhase, $"{remaining} kayıt kaldı.", DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(token);
    }

    private static T? DecodePayload<T>(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return default;
        try { return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(Convert.FromBase64String(payload)), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
        catch (Exception exception) when (exception is FormatException or JsonException) { return default; }
    }

    private static bool ValidTranslation(TranslationItem item) =>
        item.SourceArticleId != Guid.Empty && item.Slug is { Length: <= 240 } && SlugPattern().IsMatch(item.Slug) &&
        !string.IsNullOrWhiteSpace(item.Title) && item.Title.Length <= 180 && item.Summary?.Length <= 500 && !string.IsNullOrWhiteSpace(item.Body);

    private static string SanitizeBody(string? body)
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear(); foreach (var tag in new[] { "p","br","h2","h3","h4","strong","em","u","s","blockquote","ul","ol","li","pre","code","a","img","figure","figcaption","hr","table","thead","tbody","tfoot","tr","th","td","video","audio","source" }) sanitizer.AllowedTags.Add(tag);
        sanitizer.AllowedAttributes.Clear(); foreach (var attribute in new[] { "href","target","rel","src","alt","title","width","height","colspan","rowspan","controls","poster","preload" }) sanitizer.AllowedAttributes.Add(attribute);
        sanitizer.AllowedSchemes.Clear(); foreach (var scheme in new[] { "http", "https" }) sanitizer.AllowedSchemes.Add(scheme);
        return sanitizer.Sanitize(body ?? "");
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)] private static partial Regex SlugPattern();
    private sealed record EncodedPayload(string? PayloadBase64);
    private sealed record TranslationBatch(TranslationItem[] Items);
    private sealed record TranslationItem(Guid SourceArticleId, string Locale, string Slug, string Title, string Summary, string Body);
    private sealed record SeoBatch(SeoItem[] Items);
    private sealed record SeoItem(Guid ArticleId, string SeoTitle, string SeoDescription);
}
