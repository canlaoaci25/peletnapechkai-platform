using System.Security.Cryptography;
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
using SkiaSharp;

namespace Peletnapechkai.Api.Endpoints;

public static partial class AutomationWorkerEndpoints
{
    private static async Task<IResult> GetCandidatesAsync(Guid id, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        if (job.Status != AutomationJobStatus.Running) return Results.Conflict(new { message = "İş çalışır durumda değil." });

        if (job.Type == AutomationJobType.ReadyContentGeneration)
            return await GetReadyContentCandidatesAsync(job, database, token);

        if (job.Type == AutomationJobType.CategoryLocalization)
        {
            var sources = await database.Categories.AsNoTracking().Where(item => item.Locale.IsDefault).OrderBy(item => item.Id).Select(item => new { item.Id, item.Slug, item.Name, item.Description }).ToArrayAsync(token);
            var sourceIds = sources.Select(item => item.Id).ToArray();
            var translated = await database.Categories.AsNoTracking().Where(category => category.SourceCategoryId != null && sourceIds.Contains(category.SourceCategoryId.Value))
                .Select(category => new { SourceId = category.SourceCategoryId!.Value, Locale = category.Locale.Code }).ToArrayAsync(token);
            var completed = translated.Select(item => (item.SourceId, item.Locale)).ToHashSet();
            var candidates = (from source in sources from locale in job.TargetLocales where !completed.Contains((source.Id, locale)) select new { sourceCategoryId = source.Id, source.Slug, source.Name, source.Description, locale }).Take(10).ToArray();
            return Results.Ok(new { kind = "category", candidates });
        }

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
            var candidates = await database.ArticleLocalizations.AsNoTracking().Where(article => job.TargetLocales.Contains(article.Locale.Code) && article.Status == PublicationStatus.Published && (article.SeoTitle == null || article.SeoDescription == null))
                .OrderBy(article => article.Id).Take(10).Select(article => new { article.Id, locale = article.Locale.Code, article.Slug, article.Title, article.Summary, article.Body }).ToArrayAsync(token);
            return Results.Ok(new { kind = "seo", candidates });
        }

        return Results.Conflict(new { message = "Bu iş türü yapılandırılmış içerik adayı sağlamaz." });
    }

    private static async Task<IResult> GetReadyContentCandidatesAsync(AutomationJob job, PublishingDbContext database, CancellationToken token)
    {
        var sources = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.GeneratedByAutomationJobId == job.Id && article.Locale.IsDefault && article.Status == PublicationStatus.Published)
            .OrderBy(article => article.Id).Select(article => new { article.Id, article.ArticleGroupId, article.Slug, article.Title, article.Summary, article.Body }).ToListAsync(token);
        if (sources.Count > 0 && job.AutoTranslate)
        {
            var existing = await database.ArticleLocalizations.AsNoTracking().Where(article => article.GeneratedByAutomationJobId == job.Id && !article.Locale.IsDefault && article.Status != PublicationStatus.Archived)
                .Select(article => new { article.ArticleGroupId, Locale = article.Locale.Code }).ToListAsync(token);
            var keys = existing.Select(item => $"{item.ArticleGroupId:N}|{item.Locale}").ToHashSet(StringComparer.OrdinalIgnoreCase);
            var candidates = (from source in sources from locale in job.TargetLocales where !keys.Contains($"{source.ArticleGroupId:N}|{locale}")
                              select new { sourceArticleId = source.Id, sourceArticleGroupId = source.ArticleGroupId, source.Slug, source.Title, source.Summary, source.Body, locale }).Take(job.TargetLocales.Length).ToArray();
            if (candidates.Length > 0)
            {
                var total = sources.Count * job.TargetLocales.Length; var done = existing.Count;
                job.ReportProgress(sources.Count, 0, 4, $"Çeviri fazı: {done}/{total} tamamlandı, {total - done} çeviri kaldı.", DateTimeOffset.UtcNow);
                await database.SaveChangesAsync(token); return Results.Ok(new { kind = "translation", candidates });
            }
        }

        if (sources.Count > 0 && job.AutoSeo)
        {
            var candidates = await database.ArticleLocalizations.AsNoTracking()
                .Where(article => article.GeneratedByAutomationJobId == job.Id && article.Status == PublicationStatus.Published && (article.SeoTitle == null || article.SeoDescription == null))
                .OrderBy(article => article.Id).Take(10).Select(article => new { article.Id, locale = article.Locale.Code, article.Slug, article.Title, article.Summary, article.Body }).ToArrayAsync(token);
            if (candidates.Length > 0)
            {
                var remaining = await database.ArticleLocalizations.AsNoTracking().CountAsync(article => article.GeneratedByAutomationJobId == job.Id && article.Status == PublicationStatus.Published && (article.SeoTitle == null || article.SeoDescription == null), token);
                job.ReportProgress(sources.Count, 0, 5, $"SEO fazı: {remaining} makalenin SEO alanları kaldı.", DateTimeOffset.UtcNow);
                await database.SaveChangesAsync(token); return Results.Ok(new { kind = "seo", candidates });
            }
        }

        if (sources.Count < job.TotalItems)
        {
            var category = await database.Categories.AsNoTracking().Where(item => item.Id == job.CategoryId)
                .Select(item => new { item.Id, item.Name, item.Slug }).SingleAsync(token);
            var existing = await database.ArticleLocalizations.AsNoTracking().Where(article => article.Locale.IsDefault && article.Status != PublicationStatus.Archived)
                .OrderByDescending(article => article.CreatedAt).Take(300).Select(article => new { article.Title, article.Summary, article.Slug }).ToArrayAsync(token);
            job.ReportProgress(sources.Count, 0, 2, $"Makale aşaması {sources.Count + 1}/{job.TotalItems}: tek ve kapsamlı Türkçe içerik araştırılıyor.", DateTimeOffset.UtcNow);
            await database.SaveChangesAsync(token);
            return Results.Ok(new { kind = "generation", requestedCount = 1, category, articleType = job.RequestedArticleType, includeImages = job.IncludeImages, autoSeo = job.AutoSeo, existing });
        }

        job.ReportProgress(sources.Count, 0, 6, "Son doğrulama tamamlandı; tüm seçili fazlar eksiksiz.", DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(token);
        return Results.Ok(new { kind = "complete", candidates = Array.Empty<object>() });
    }

    private static async Task<IResult> SaveTranslationsAsync(Guid id, EncodedPayload request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        if (job.Type is not (AutomationJobType.ContentTranslation or AutomationJobType.ReadyContentGeneration) || job.Status != AutomationJobStatus.Running) return Results.Conflict();
        var payload = DecodePayload<TranslationBatch>(request.PayloadBase64);
        if (payload?.Items is not { Length: > 0 and <= 5 }) return Results.BadRequest(new { message = "Geçersiz çeviri paketi." });
        var now = DateTimeOffset.UtcNow;
        foreach (var item in payload.Items)
        {
            if (!job.TargetLocales.Contains(item.Locale, StringComparer.OrdinalIgnoreCase) || !ValidTranslation(item)) return Results.BadRequest(new { message = "Çeviri alanları geçersiz." });
            var source = await database.ArticleLocalizations.Include(article => article.ArticleGroup).Include(article => article.CoverMediaAsset).Include(article => article.Categories).SingleOrDefaultAsync(article => article.Id == item.SourceArticleId && article.Locale.IsDefault && article.Status == PublicationStatus.Published && (job.Type != AutomationJobType.ReadyContentGeneration || article.GeneratedByAutomationJobId == job.Id), token);
            var locale = await database.Locales.SingleOrDefaultAsync(candidate => candidate.Code == item.Locale && candidate.IsEnabled, token);
            if (source is null || locale is null) return Results.BadRequest(new { message = "Kaynak veya hedef dil bulunamadı." });
            var exists = await database.ArticleLocalizations.AnyAsync(article => article.ArticleGroupId == source.ArticleGroupId && article.LocaleId == locale.Id && article.Status != PublicationStatus.Archived, token);
            if (exists) continue;
            var slug = item.Slug;
            if (await database.ArticleLocalizations.AnyAsync(article => article.LocaleId == locale.Id && article.Slug == slug, token))
                slug = $"{slug[..Math.Min(slug.Length, 230)]}-{source.ArticleGroupId.ToString("N")[..8]}";
            var article = new ArticleLocalization(source.ArticleGroup, locale, slug, item.Title, item.Summary, SanitizeBody(item.Body), now);
            var sourceCategoryIds = source.Categories.Select(category => category.Id).ToArray();
            if (sourceCategoryIds.Length > 0)
            {
                var translatedCategories = await database.Categories.Where(category => category.LocaleId == locale.Id && category.SourceCategoryId != null && sourceCategoryIds.Contains(category.SourceCategoryId.Value)).ToListAsync(token);
                foreach (var translatedCategory in translatedCategories) article.Categories.Add(translatedCategory);
            }
            if (job.Type == AutomationJobType.ReadyContentGeneration) article.MarkGeneratedTranslation(job.Id);
            if (source.CoverMediaAsset is not null) article.UpdateCover(source.CoverMediaAsset, item.Title, source.CoverCaption, source.CoverCredit, now);
            article.PublishAutomatedTranslation(now);
            database.ArticleLocalizations.Add(article);
            database.AuditLogs.Add(new AuditLog(job.CreatedByUserId, "automation.translation_published", nameof(ArticleLocalization), article.Id, JsonSerializer.Serialize(new { sourceArticleId = source.Id, item.Locale, jobId = job.Id }), now));
        }
        await database.SaveChangesAsync(token);
        await UpdateProgressAsync(job, database, token);
        return Results.Ok();
    }

    private static async Task<IResult> SaveCategoryTranslationsAsync(Guid id, EncodedPayload request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job?.Type != AutomationJobType.CategoryLocalization || job.Status != AutomationJobStatus.Running) return Results.Conflict();
        var payload = DecodePayload<CategoryTranslationBatch>(request.PayloadBase64);
        if (payload?.Items is not { Length: > 0 and <= 10 }) return Results.BadRequest(new { message = "Geçersiz kategori çeviri paketi." });
        foreach (var item in payload.Items)
        {
            if (!job.TargetLocales.Contains(item.Locale, StringComparer.OrdinalIgnoreCase) || item.SourceCategoryId == Guid.Empty || !SlugPattern().IsMatch(item.Slug) || string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 160) return Results.BadRequest(new { message = "Kategori çeviri alanları geçersiz." });
            var source = await database.Categories.AsNoTracking().SingleOrDefaultAsync(category => category.Id == item.SourceCategoryId && category.Locale.IsDefault, token);
            var locale = await database.Locales.SingleOrDefaultAsync(candidate => candidate.Code == item.Locale && candidate.IsEnabled && !candidate.IsDefault, token);
            if (source is null || locale is null) return Results.BadRequest(new { message = "Kaynak kategori veya hedef dil bulunamadı." });
            var existingTranslation = await database.Categories.AnyAsync(category => category.SourceCategoryId == source.Id && category.LocaleId == locale.Id, token);
            if (existingTranslation) continue;
            var slug = item.Slug;
            if (await database.Categories.AnyAsync(category => category.LocaleId == locale.Id && category.Slug == slug, token)) slug = $"{slug[..Math.Min(slug.Length, 150)]}-{source.Id.ToString("N")[..8]}";
            var translated = new Category(locale, slug, item.Name, DateTimeOffset.UtcNow);
            translated.LinkTranslationSource(source);
            database.Categories.Add(translated);
            database.AuditLogs.Add(new AuditLog(job.CreatedByUserId, "automation.category_localized", nameof(Category), source.Id, JsonSerializer.Serialize(new { locale = item.Locale, translatedCategoryId = translated.Id }), DateTimeOffset.UtcNow));
        }
        await database.SaveChangesAsync(token);
        var remaining = await AutomationCandidateCounter.CountMissingCategoryTranslationsAsync(database, job.TargetLocales, token);
        job.ReportProgress(Math.Max(0, job.TotalItems - remaining), 0, job.CurrentPhase, $"Kategori çevirisi: {remaining} kayıt kaldı.", DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(token); return Results.Ok();
    }

    private static string? CategoryLocale(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonDocument.Parse(json).RootElement.GetProperty("locale").GetString(); } catch { return null; }
    }

    private static async Task<IResult> SaveSeoDraftsAsync(Guid id, EncodedPayload request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        if (job.Type is not (AutomationJobType.SeoLocalization or AutomationJobType.ReadyContentGeneration) || job.Status != AutomationJobStatus.Running) return Results.Conflict();
        var payload = DecodePayload<SeoBatch>(request.PayloadBase64);
        if (payload?.Items is not { Length: > 0 and <= 10 }) return Results.BadRequest(new { message = "Geçersiz SEO paketi." });
        var now = DateTimeOffset.UtcNow;
        foreach (var item in payload.Items)
        {
            if (string.IsNullOrWhiteSpace(item.SeoTitle) || item.SeoTitle.Length > 180 || string.IsNullOrWhiteSpace(item.SeoDescription) || item.SeoDescription.Length > 320) return Results.BadRequest(new { message = "SEO alanları geçersiz." });
            var article = await database.ArticleLocalizations.Include(candidate => candidate.Locale).SingleOrDefaultAsync(candidate => candidate.Id == item.ArticleId && (job.Type == AutomationJobType.ReadyContentGeneration || job.TargetLocales.Contains(candidate.Locale.Code)), token);
            if (article is null || article.Status != PublicationStatus.Published || (job.Type == AutomationJobType.SeoLocalization && article.Locale.IsDefault) || (job.Type == AutomationJobType.ReadyContentGeneration && article.GeneratedByAutomationJobId != job.Id)) return Results.BadRequest(new { message = "SEO hedef içeriği bulunamadı." });
            if (job.Type == AutomationJobType.ReadyContentGeneration) article.UpdateGeneratedSeo(job.Id, item.SeoTitle, item.SeoDescription, now);
            else article.UpdateAutomatedSeo(item.SeoTitle, item.SeoDescription, now);
            database.AuditLogs.Add(new AuditLog(job.CreatedByUserId, "automation.seo_localized", nameof(ArticleLocalization), article.Id, JsonSerializer.Serialize(new { job.Id }), now));
        }
        await database.SaveChangesAsync(token);
        await UpdateProgressAsync(job, database, token);
        return Results.Ok();
    }

    private static async Task<IResult> PublishExistingTranslationsAsync(HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var automatedIds = await database.AuditLogs.AsNoTracking()
            .Where(log => log.Action == "automation.translation_draft_created" && log.EntityType == nameof(ArticleLocalization))
            .Select(log => log.EntityId).Distinct().ToArrayAsync(token);
        if (automatedIds.Length == 0) return Results.Ok(new { published = 0 });

        var articles = await database.ArticleLocalizations
            .Include(article => article.Locale)
            .Include(article => article.ArticleGroup).ThenInclude(group => group.Localizations).ThenInclude(article => article.Locale)
            .Where(article => automatedIds.Contains(article.Id) && article.Status == PublicationStatus.Draft && !article.Locale.IsDefault)
            .ToListAsync(token);
        var now = DateTimeOffset.UtcNow;
        var published = 0;
        foreach (var article in articles)
        {
            var source = article.ArticleGroup.Localizations.SingleOrDefault(candidate => candidate.Locale.IsDefault && candidate.Status == PublicationStatus.Published);
            if (source is null) continue;
            article.PublishAutomatedTranslation(now);
            database.AuditLogs.Add(new AuditLog(null, "automation.translation_published", nameof(ArticleLocalization), article.Id, JsonSerializer.Serialize(new { sourceArticleId = source.Id, migratedFromDraft = true }), now));
            published++;
        }
        await database.SaveChangesAsync(token);
        return Results.Ok(new { published });
    }

    private static async Task<IResult> SaveGeneratedContentAsync(Guid id, EncodedPayload request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null) return Results.NotFound();
        if (job.Type != AutomationJobType.ReadyContentGeneration || job.Status != AutomationJobStatus.Running || job.CategoryId is null || !Enum.TryParse<ArticleType>(job.RequestedArticleType, out var articleType)) return Results.Conflict();
        var payload = DecodePayload<GeneratedContentBatch>(request.PayloadBase64);
        var alreadyCreated = await database.ArticleLocalizations.CountAsync(article => article.GeneratedByAutomationJobId == job.Id && article.Locale.IsDefault && article.Status != PublicationStatus.Archived, token);
        var remaining = job.TotalItems - alreadyCreated;
        if (payload?.Items is not { Length: 1 } || payload.Items.Length > remaining) return Results.BadRequest(new { message = "Her aşamada tam olarak bir hazır içerik gereklidir." });
        var locale = await database.Locales.SingleAsync(item => item.IsDefault && item.IsEnabled, token);
        var category = await database.Categories.SingleOrDefaultAsync(item => item.Id == job.CategoryId && item.LocaleId == locale.Id, token);
        if (category is null) return Results.BadRequest(new { message = "Kategori bulunamadı." });
        var comparisons = await database.ArticleLocalizations.AsNoTracking().Where(article => article.LocaleId == locale.Id && article.Status != PublicationStatus.Archived)
            .Select(article => article.Title + " " + article.Summary).ToListAsync(token);
        var now = DateTimeOffset.UtcNow;
        foreach (var item in payload.Items)
        {
            if (!ValidGeneratedContent(item, job.AutoSeo) || comparisons.Any(existing => Similarity(existing, item.Title + " " + item.Summary) >= 0.52))
                return Results.Conflict(new { message = $"'{item.Title}' mevcut veya aynı paketteki bir içeriğe fazla benziyor." });
            if (await database.ArticleLocalizations.AnyAsync(article => article.LocaleId == locale.Id && article.Slug == item.Slug, token)) return Results.Conflict(new { message = "Üretilen URL kısa adı zaten kullanılıyor." });
            var sanitizedBody = SanitizeBody(item.Body);
            if (Regex.Replace(sanitizedBody, "<[^>]+>", " ").Length < 1800) return Results.BadRequest(new { message = "Makale gövdesi temizleme sonrasında yeterince ayrıntılı değil." });
            var group = new ArticleGroup(articleType, now);
            foreach (var sourceItem in item.Sources)
            {
                if (!Uri.TryCreate(sourceItem.Url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https")) return Results.BadRequest(new { message = "Kaynak adresi geçersiz." });
                var canonical = uri.AbsoluteUri;
                var source = await database.Sources.SingleOrDefaultAsync(candidate => candidate.Url == canonical, token) ?? new Source(sourceItem.Name, uri, now);
                if (database.Entry(source).State == EntityState.Detached) database.Sources.Add(source);
                group.Sources.Add(source);
            }
            GeneratedCover? cover = null;
            if (job.IncludeImages)
            {
                if (item.InlineImageQueries is not { Length: 2 } || item.InlineImageAltTexts is not { Length: 2 }) return Results.BadRequest(new { message = "Resimli makalede iki gövde görseli gereklidir." });
                cover = await CreateGeneratedCoverAsync(item.ImageSearchQuery ?? item.Title, category.Name, configuration, now, token);
                database.MediaAssets.Add(cover.Asset); group.MediaAssets.Add(cover.Asset);
                var inlineAssets = new List<MediaAsset>(2);
                for (var imageIndex = 0; imageIndex < 2; imageIndex++)
                {
                    var inline = await CreateGeneratedInlineAsync(item.InlineImageQueries[imageIndex], category.Name, imageIndex, configuration, now, token);
                    database.MediaAssets.Add(inline); group.MediaAssets.Add(inline); inlineAssets.Add(inline);
                }
                sanitizedBody = InsertInlineImages(sanitizedBody, inlineAssets, item.InlineImageAltTexts);
            }
            var article = new ArticleLocalization(group, locale, item.Slug, item.Title, item.Summary, sanitizedBody, now);
            article.Categories.Add(category);
            if (job.AutoSeo) article.UpdateDraft(article.Slug, article.Title, article.Summary, article.Body, item.SeoTitle, item.SeoDescription, now);
            if (cover is not null) article.UpdateCover(cover.Asset, item.ImageAltText ?? item.Title, cover.SourceUrl, cover.Credit, now);
            article.PublishAutomatedSource(job.Id, now);
            database.ArticleGroups.Add(group); database.ArticleLocalizations.Add(article);
            database.AuditLogs.Add(new AuditLog(job.CreatedByUserId, "automation.ready_content_published", nameof(ArticleLocalization), article.Id, JsonSerializer.Serialize(new { jobId = job.Id, categoryId = category.Id, articleType, sourceCount = item.Sources.Length, job.IncludeImages }), now));
            comparisons.Add(item.Title + " " + item.Summary);
        }
        await database.SaveChangesAsync(token);
        var completed = alreadyCreated + payload.Items.Length;
        var phase = completed == job.TotalItems && job.IncludeImages ? 3 : 2;
        job.ReportProgress(completed, 0, phase, $"Makale aşaması {completed}/{job.TotalItems}: Türkçe yayın hazır; çeviri ve SEO tamamlanıyor.", DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(token);
        return Results.Ok(new { completed, remaining = job.TotalItems - completed });
    }

    private static async Task UpdateProgressAsync(AutomationJob job, PublishingDbContext database, CancellationToken token)
    {
        if (job.Type == AutomationJobType.ReadyContentGeneration)
        {
            var generated = await database.ArticleLocalizations.AsNoTracking().CountAsync(article => article.GeneratedByAutomationJobId == job.Id && article.Locale.IsDefault && article.Status == PublicationStatus.Published, token);
            var translated = await database.ArticleLocalizations.AsNoTracking().CountAsync(article => article.GeneratedByAutomationJobId == job.Id && !article.Locale.IsDefault && article.Status == PublicationStatus.Published, token);
            var translationTotal = generated * job.TargetLocales.Length;
            var seoRemaining = job.AutoSeo ? await database.ArticleLocalizations.AsNoTracking().CountAsync(article => article.GeneratedByAutomationJobId == job.Id && article.Status == PublicationStatus.Published && (article.SeoTitle == null || article.SeoDescription == null), token) : 0;
            var phase = job.AutoTranslate && translated < translationTotal ? 4 : job.AutoSeo && seoRemaining > 0 ? 5 : 6;
            var message = phase == 4 ? $"Çeviri fazı: {translated}/{translationTotal} tamamlandı, {translationTotal - translated} kaldı." : phase == 5 ? $"SEO fazı: {seoRemaining} makale kaldı." : "Seçili üretim fazları tamamlandı.";
            job.ReportProgress(generated, 0, phase, message, DateTimeOffset.UtcNow); await database.SaveChangesAsync(token); return;
        }
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

    private static bool ValidGeneratedContent(GeneratedContentItem item, bool requireSeo) =>
        item.Slug is { Length: <= 240 } && SlugPattern().IsMatch(item.Slug) &&
        item.Title is { Length: >= 20 and <= 180 } && item.Summary is { Length: >= 80 and <= 500 } &&
        item.Body is { Length: >= 2500 } && item.Sources is not null &&
        GeneratedSourceQualityPolicy.IsValid(item.Sources.Select(source => ((string?)source.Name, (string?)source.Url))) &&
        (!requireSeo || item.SeoTitle is { Length: > 0 and <= 180 } && item.SeoDescription is { Length: > 0 and <= 320 });

    private static double Similarity(string left, string right)
    {
        static HashSet<string> Tokens(string value) => Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9çğıöşü]+", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(token => token.Length > 2).ToHashSet(StringComparer.Ordinal);
        var a = Tokens(left); var b = Tokens(right); if (a.Count == 0 || b.Count == 0) return 0;
        var intersection = a.Intersect(b).Count(); var union = a.Union(b).Count(); return union == 0 ? 0 : (double)intersection / union;
    }

    private static async Task<GeneratedCover> CreateGeneratedCoverAsync(string title, string category, IConfiguration configuration, DateTimeOffset now, CancellationToken token)
    {
        var root = Path.GetFullPath(configuration["Media:StoragePath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BOECL", "Media"));
        var key = Path.Combine(now.ToString("yyyy"), now.ToString("MM"), $"{Guid.CreateVersion7()}-ai-cover.webp");
        var path = Path.GetFullPath(Path.Combine(root, key)); if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid media path.");
        var attribution = await WriteTextlessCoverAsync(path, title + " " + category, false, configuration, false, token);
        var length = new FileInfo(path).Length; var normalizedKey = key.Replace('\\', '/');
        var asset = new MediaAsset(normalizedKey, "boecl-ai-cover.webp", "image/webp", length, now); asset.SetImageMetadata(width, height, normalizedKey, length); return new GeneratedCover(asset, attribution.Credit, attribution.SourceUrl);
    }

    private static async Task<MediaAsset> CreateGeneratedInlineAsync(string query, string category, int index, IConfiguration configuration, DateTimeOffset now, CancellationToken token)
    {
        var root = Path.GetFullPath(configuration["Media:StoragePath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BOECL", "Media"));
        var key = Path.Combine(now.ToString("yyyy"), now.ToString("MM"), $"{Guid.CreateVersion7()}-inline-{index + 1}.webp");
        var path = Path.GetFullPath(Path.Combine(root, key)); if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid media path.");
        await WriteTextlessCoverAsync(path, query + " " + category, false, configuration, false, token);
        var length = new FileInfo(path).Length; var normalizedKey = key.Replace('\\', '/');
        var asset = new MediaAsset(normalizedKey, $"boecl-inline-{index + 1}.webp", "image/webp", length, now); asset.SetImageMetadata(width, height, normalizedKey, length); return asset;
    }

    private static string InsertInlineImages(string body, IReadOnlyList<MediaAsset> assets, IReadOnlyList<string> altTexts)
    {
        var figures = assets.Select((asset, index) => $"<figure class=\"article-inline-image\"><img src=\"/api/media/{asset.Id}?v={asset.OptimizedByteLength}\" alt=\"{System.Net.WebUtility.HtmlEncode(altTexts[index])}\" width=\"1200\" height=\"675\" loading=\"lazy\"></figure>").ToArray();
        var next = 0;
        var result = Regex.Replace(body, "</h2>", match => next < figures.Length ? match.Value + figures[next++] : match.Value, RegexOptions.IgnoreCase);
        while (next < figures.Length) result += figures[next++];
        return result;
    }

    private static async Task<IResult> RefreshGeneratedCoversAsync(Guid id, CoverRefreshRequest? request, HttpContext context, PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var job = await database.AutomationJobs.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && item.Type == AutomationJobType.ReadyContentGeneration, token);
        if (job is null) return Results.NotFound();
        var articles = await database.ArticleLocalizations.Include(item => item.Locale).Include(item => item.Categories).Include(item => item.CoverMediaAsset)
            .Include(item => item.ArticleGroup).ThenInclude(group => group.MediaAssets)
            .Where(item => item.GeneratedByAutomationJobId == id && item.Locale.IsDefault && item.CoverMediaAsset != null).ToListAsync(token);
        var root = Path.GetFullPath(configuration["Media:StoragePath"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "BOECL", "Media"));
        var refreshed = 0;
        foreach (var article in articles.GroupBy(item => item.CoverMediaAssetId).Select(group => group.First()))
        {
            var path = Path.GetFullPath(Path.Combine(root, article.CoverMediaAsset!.StorageKey));
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return Results.BadRequest();
            var requestedQuery = request?.Queries?.GetValueOrDefault(article.Id);
            var query = string.IsNullOrWhiteSpace(requestedQuery) ? article.Title + " " + string.Join(' ', article.Categories.Select(item => item.Name)) : requestedQuery.Trim();
            var attribution = await WriteTextlessCoverAsync(path, query, true, configuration, false, token);
            var inlineIndex = 0;
            foreach (var inline in article.ArticleGroup.MediaAssets.Where(asset => asset.Id != article.CoverMediaAssetId))
            {
                var inlinePath = Path.GetFullPath(Path.Combine(root, inline.StorageKey));
                if (!inlinePath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
                await WriteTextlessCoverAsync(inlinePath, $"{article.Title} {string.Join(' ', article.Categories.Select(item => item.Name))} scene {++inlineIndex}", true, configuration, false, token);
            }
            var siblings = await database.ArticleLocalizations.Where(item => item.GeneratedByAutomationJobId == id && item.CoverMediaAssetId == article.CoverMediaAssetId).ToListAsync(token);
            foreach (var sibling in siblings) sibling.RefreshGeneratedCover(id, article.CoverMediaAsset, sibling.CoverAltText ?? sibling.Title, attribution.SourceUrl, attribution.Credit, DateTimeOffset.UtcNow);
            refreshed++;
        }
        await database.SaveChangesAsync(token);
        return Results.Ok(new { refreshed });
    }

    private const int width = 1200, height = 675;
    private static async Task<CoverAttribution> WriteTextlessCoverAsync(string path, string seed, bool overwrite, IConfiguration configuration, bool allowStockProvider, CancellationToken token)
    {
        var pexelsKey = configuration["Media:PexelsApiKey"];
        if (allowStockProvider && !string.IsNullOrWhiteSpace(pexelsKey))
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(25) };
                client.DefaultRequestHeaders.Add("Authorization", pexelsKey);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("BOECL/1.0 (+https://peletnapechkai.com)");
                var query = Uri.EscapeDataString(seed.Length > 120 ? seed[..120] : seed);
                using var response = await client.GetAsync($"https://api.pexels.com/v1/search?query={query}&orientation=landscape&size=large&per_page=5", token);
                response.EnsureSuccessStatusCode();
                using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(token));
                var photos = document.RootElement.GetProperty("photos");
                if (photos.GetArrayLength() > 0)
                {
                    // Pexels orders results by relevance. Random selection among the first
                    // page made visually unrelated photos much more likely.
                    var photo = photos[0];
                    var source = photo.GetProperty("src");
                    var imageUrl = source.TryGetProperty("large2x", out var large2x) ? large2x.GetString() : source.GetProperty("large").GetString();
                    if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var imageUri))
                    {
                        var bytes = await client.GetByteArrayAsync(imageUri, token);
                        if (bytes.Length is > 0 and <= 15_000_000)
                        {
                            using var bitmap = SKBitmap.Decode(bytes) ?? throw new InvalidDataException("Pexels image could not be decoded.");
                            await SaveCoverBitmapAsync(path, bitmap, overwrite, token);
                            var photographer = photo.GetProperty("photographer").GetString() ?? "Pexels içerik üreticisi";
                            return new CoverAttribution($"Fotoğraf: {photographer} / Pexels", photo.GetProperty("url").GetString());
                        }
                    }
                }
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidDataException or KeyNotFoundException) { }
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        var background = new SKColor((byte)(18 + hash[0] % 30), (byte)(22 + hash[1] % 34), (byte)(34 + hash[2] % 44));
        var accent = new SKColor((byte)(110 + hash[3] % 130), (byte)(80 + hash[4] % 150), (byte)(90 + hash[5] % 145));
        using var surface = SKSurface.Create(new SKImageInfo(width, height)); var canvas = surface.Canvas; canvas.Clear(background);
        using var glow = new SKPaint { Color = accent.WithAlpha(150), IsAntialias = true, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 55) };
        using var shape = new SKPaint { Color = accent.WithAlpha(180), IsAntialias = true };
        using var soft = new SKPaint { Color = SKColors.White.WithAlpha(28), IsAntialias = true };
        canvas.DrawCircle(180 + hash[6] * 3, 80 + hash[7] * 2, 210 + hash[8] % 150, glow);
        canvas.DrawCircle(900 + hash[9], 420 + hash[10], 150 + hash[11] % 180, shape);
        canvas.Save(); canvas.RotateDegrees(-12 + hash[12] % 25, 600, 338);
        for (var index = 0; index < 5; index++) canvas.DrawRoundRect(90 + index * 205, 120 + (hash[13 + index] % 170), 170, 330, 38, 38, soft);
        canvas.Restore();
        if (seed.Contains("anime", StringComparison.OrdinalIgnoreCase))
        {
            using var ink = new SKPaint { Color = SKColors.White.WithAlpha(175), IsAntialias = true, StrokeWidth = 7, Style = SKPaintStyle.Stroke };
            using var silhouette = new SKPaint { Color = SKColors.Black.WithAlpha(155), IsAntialias = true };
            var focus = new SKPoint(600, 320);
            for (var ray = 0; ray < 28; ray++)
            {
                var angle = (float)(ray * Math.PI * 2 / 28); var inner = 245 + hash[ray % hash.Length] % 75;
                canvas.DrawLine(focus.X + MathF.Cos(angle) * inner, focus.Y + MathF.Sin(angle) * inner,
                    focus.X + MathF.Cos(angle) * 690, focus.Y + MathF.Sin(angle) * 690, ink);
            }
            canvas.DrawOval(new SKRect(515, 105, 685, 285), silhouette);
            using var hair = new SKPath(); hair.MoveTo(505, 180); hair.LineTo(530, 65); hair.LineTo(575, 115); hair.LineTo(620, 55); hair.LineTo(650, 125); hair.LineTo(705, 95); hair.LineTo(690, 210); hair.Close(); canvas.DrawPath(hair, silhouette);
            using var body = new SKPath(); body.MoveTo(470, 665); body.LineTo(505, 310); body.QuadTo(600, 245, 695, 310); body.LineTo(735, 665); body.Close(); canvas.DrawPath(body, silhouette);
        }
        using var image = surface.Snapshot(); using var generatedBitmap = SKBitmap.FromImage(image);
        await SaveCoverBitmapAsync(path, generatedBitmap, overwrite, token);
        return new CoverAttribution("BOECL yazısız otomatik görsel", null);
    }

    private static async Task SaveCoverBitmapAsync(string path, SKBitmap bitmap, bool overwrite, CancellationToken token)
    {
        var scale = Math.Max((float)width / bitmap.Width, (float)height / bitmap.Height);
        var scaledWidth = (int)Math.Ceiling(bitmap.Width * scale); var scaledHeight = (int)Math.Ceiling(bitmap.Height * scale);
        using var resized = bitmap.Resize(new SKImageInfo(scaledWidth, scaledHeight), SKSamplingOptions.Default) ?? throw new InvalidDataException("Image resize failed.");
        using var cropped = new SKBitmap(width, height); using (var canvas = new SKCanvas(cropped)) canvas.DrawBitmap(resized, (width - scaledWidth) / 2f, (height - scaledHeight) / 2f, SKSamplingOptions.Default, null);
        using var image = SKImage.FromBitmap(cropped); using var data = image.Encode(SKEncodedImageFormat.Webp, 88);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        data.SaveTo(stream); await stream.FlushAsync(token);
    }

    private sealed record GeneratedCover(MediaAsset Asset, string Credit, string? SourceUrl);
    private sealed record CoverAttribution(string Credit, string? SourceUrl);
    private sealed record CoverRefreshRequest(Dictionary<Guid, string>? Queries);

    private static string SanitizeBody(string? body)
    {
        body ??= "";
        if (!body.TrimStart().StartsWith('<')) body = MarkdownToHtml(body);
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear(); foreach (var tag in new[] { "p","br","h2","h3","h4","strong","em","u","s","blockquote","ul","ol","li","pre","code","a","img","figure","figcaption","hr","table","thead","tbody","tfoot","tr","th","td","video","audio","source" }) sanitizer.AllowedTags.Add(tag);
        sanitizer.AllowedAttributes.Clear(); foreach (var attribute in new[] { "href","target","rel","src","alt","title","width","height","colspan","rowspan","controls","poster","preload" }) sanitizer.AllowedAttributes.Add(attribute);
        sanitizer.AllowedSchemes.Clear(); foreach (var scheme in new[] { "http", "https" }) sanitizer.AllowedSchemes.Add(scheme);
        return sanitizer.Sanitize(body);
    }

    private static string MarkdownToHtml(string markdown)
    {
        var html = new StringBuilder();
        var paragraph = new List<string>();
        var listType = "";
        void FlushParagraph()
        {
            if (paragraph.Count == 0) return;
            html.Append("<p>").Append(string.Join(" ", paragraph.Select(System.Net.WebUtility.HtmlEncode))).Append("</p>");
            paragraph.Clear();
        }
        void CloseList()
        {
            if (listType.Length == 0) return;
            html.Append("</").Append(listType).Append('>');
            listType = "";
        }

        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) { FlushParagraph(); CloseList(); continue; }
            var heading = Regex.Match(line, "^(#{2,4})\\s+(.+)$");
            if (heading.Success)
            {
                FlushParagraph(); CloseList(); var level = heading.Groups[1].Value.Length;
                html.Append("<h").Append(level).Append('>').Append(System.Net.WebUtility.HtmlEncode(heading.Groups[2].Value)).Append("</h").Append(level).Append('>');
                continue;
            }
            var bullet = Regex.Match(line, "^[-*]\\s+(.+)$");
            var numbered = Regex.Match(line, "^\\d+[.)]\\s+(.+)$");
            if (bullet.Success || numbered.Success)
            {
                FlushParagraph(); var requested = bullet.Success ? "ul" : "ol";
                if (listType != requested) { CloseList(); listType = requested; html.Append('<').Append(listType).Append('>'); }
                html.Append("<li>").Append(System.Net.WebUtility.HtmlEncode((bullet.Success ? bullet : numbered).Groups[1].Value)).Append("</li>");
                continue;
            }
            CloseList(); paragraph.Add(line);
        }
        FlushParagraph(); CloseList(); return html.ToString();
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)] private static partial Regex SlugPattern();
    private sealed record EncodedPayload(string? PayloadBase64);
    private sealed record TranslationBatch(TranslationItem[] Items);
    private sealed record TranslationItem(Guid SourceArticleId, string Locale, string Slug, string Title, string Summary, string Body);
    private sealed record CategoryTranslationBatch(CategoryTranslationItem[] Items);
    private sealed record CategoryTranslationItem(Guid SourceCategoryId, string Locale, string Slug, string Name);
    private sealed record SeoBatch(SeoItem[] Items);
    private sealed record SeoItem(Guid ArticleId, string SeoTitle, string SeoDescription);
    private sealed record GeneratedContentBatch(GeneratedContentItem[] Items);
    private sealed record GeneratedContentItem(string Slug, string Title, string Summary, string Body, string? SeoTitle, string? SeoDescription, string? ImageAltText, string? ImageSearchQuery, string[]? InlineImageAltTexts, string[]? InlineImageQueries, GeneratedSource[] Sources);
    private sealed record GeneratedSource(string Name, string Url);
}
