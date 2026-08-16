using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Automation;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class AutomationEndpoints
{
    public static IEndpointRouteBuilder MapAutomationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/automation")
            .RequireAuthorization(AuthorizationPolicies.ManageUsers)
            .WithTags("Automation");

        group.MapGet("/", ListAsync);
        group.MapGet("/{id:guid}", DetailAsync);
        group.MapGet("/scan", ScanAsync);
        group.MapGet("/visual-quality", VisualQualityAsync);
        group.MapPost("/", CreateAsync).ValidateAntiforgery();
        group.MapPost("/ready-content", CreateReadyContentAsync).ValidateAntiforgery();
        group.MapGet("/automatic-content", GetAutomaticContentAsync);
        group.MapPut("/automatic-content", UpdateAutomaticContentAsync).ValidateAntiforgery();
        group.MapPost("/{id:guid}/{action}", ChangeStateAsync).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> VisualQualityAsync(PublishingDbContext database, CancellationToken token)
    {
        var rows = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Status == PublicationStatus.Published)
            .OrderByDescending(article => article.PublishedAt)
            .Select(article => new
            {
                article.Id, locale = article.Locale.Code, article.Slug, article.Title, article.Summary, article.Body,
                article.CoverAltText, article.CoverCredit, article.PublishedAt,
                coverId = article.CoverMediaAssetId, width = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.Width,
                height = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.Height,
                optimizedBytes = article.CoverMediaAsset == null ? null : article.CoverMediaAsset.OptimizedByteLength
            }).ToListAsync(token);
        var items = rows.Select(row =>
        {
            var result = ArticleVisualQualityPolicy.Assess(new(row.Title, row.Summary, row.Body, row.CoverAltText,
                row.CoverCredit, row.width, row.height, row.optimizedBytes, row.coverId is not null));
            return new { row.Id, row.locale, row.Slug, row.Title, row.PublishedAt, score = result.Score, grade = result.Grade,
                risks = result.Risks, result.BodyImageCount, coverUrl = row.coverId is null ? null : "/api/media/" + row.coverId,
                row.CoverAltText, row.width, row.height, row.optimizedBytes };
        }).OrderBy(item => item.score).ThenByDescending(item => item.PublishedAt).ToArray();
        return Results.Ok(new
        {
            checkedAt = DateTimeOffset.UtcNow, total = items.Length, passing = items.Count(item => item.score >= 80 && item.risks.Length == 0),
            needsReview = items.Count(item => item.risks.Length > 0), missingCover = items.Count(item => item.risks.Contains("missing-cover")),
            textRisk = items.Count(item => item.risks.Contains("text-risk")), averageScore = items.Length == 0 ? 0 : Math.Round(items.Average(item => item.score), 1),
            items
        });
    }

    private static async Task<IResult> ListAsync(PublishingDbContext database, CancellationToken token) =>
        Results.Ok(await database.AutomationJobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAt)
            .Take(100)
            .Select(job => new
            {
                job.Id,
                type = job.Type.ToString(),
                status = job.Status.ToString(),
                job.TargetLocales,
                job.TotalItems,
                job.CompletedItems,
                job.FailedItems,
                job.CurrentPhase,
                job.LastMessage,
                job.CreatedAt,
                job.UpdatedAt,
                job.CompletedAt
                ,job.CategoryId, job.RequestedArticleType, job.IncludeImages, job.AutoTranslate, job.AutoSeo,
                job.IsAutomaticallyScheduled,
                categoryName = database.Categories.Where(category => category.Id == job.CategoryId).Select(category => category.Name).FirstOrDefault(),
                turkishPublished = database.ArticleLocalizations.Count(article => article.GeneratedByAutomationJobId == job.Id && article.Locale.IsDefault && article.Status == PublicationStatus.Published),
                translationPublished = database.ArticleLocalizations.Count(article => article.GeneratedByAutomationJobId == job.Id && !article.Locale.IsDefault && article.Status == PublicationStatus.Published),
                seoComplete = database.ArticleLocalizations.Count(article => article.GeneratedByAutomationJobId == job.Id && article.Status == PublicationStatus.Published && article.SeoTitle != null && article.SeoDescription != null),
                latestContentAt = database.ArticleLocalizations.Where(article => article.GeneratedByAutomationJobId == job.Id).Max(article => (DateTimeOffset?)article.CreatedAt),
                recentArticles = database.ArticleLocalizations.Where(article => article.GeneratedByAutomationJobId == job.Id && article.Locale.IsDefault).OrderByDescending(article => article.CreatedAt).Take(3).Select(article => new { article.Title, article.Slug, locale = article.Locale.Code }).ToArray()
            })
            .ToListAsync(token));

    private static async Task<IResult> DetailAsync(Guid id, PublishingDbContext database, CancellationToken token)
    {
        var job = await database.AutomationJobs.AsNoTracking()
            .Where(candidate => candidate.Id == id)
            .Select(candidate => new
            {
                candidate.Id,
                type = candidate.Type.ToString(),
                status = candidate.Status.ToString(),
                candidate.TargetLocales,
                candidate.TotalItems,
                candidate.CompletedItems,
                candidate.FailedItems,
                candidate.CurrentPhase,
                candidate.LastMessage,
                candidate.ReportText,
                candidate.CreatedAt,
                candidate.UpdatedAt,
                candidate.CompletedAt
                ,candidate.CategoryId, candidate.RequestedArticleType, candidate.IncludeImages, candidate.AutoTranslate, candidate.AutoSeo
            })
            .SingleOrDefaultAsync(token);
        return job is null ? Results.NotFound() : Results.Ok(job);
    }

    private static async Task<IResult> ScanAsync(
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        var activeLocales = await database.Locales
            .AsNoTracking()
            .Where(locale => locale.IsEnabled)
            .OrderByDescending(locale => locale.IsDefault)
            .ThenBy(locale => locale.Code)
            .Select(locale => locale.Code)
            .ToArrayAsync(token);

        var publishedArticles = await database.ArticleLocalizations
            .AsNoTracking()
            .CountAsync(article => article.Status == PublicationStatus.Published, token);
        var defaultLocale = await database.Locales.AsNoTracking().Where(locale => locale.IsDefault).Select(locale => locale.Code).SingleAsync(token);
        var targetLocales = activeLocales.Where(locale => !locale.Equals(defaultLocale, StringComparison.OrdinalIgnoreCase)).ToArray();
        var missingTranslations = await AutomationCandidateCounter.CountMissingTranslationsAsync(database, targetLocales, token);
        var seoCandidates = await AutomationCandidateCounter.CountSeoCandidatesAsync(database, targetLocales, token);
        var categoryCandidates = await AutomationCandidateCounter.CountMissingCategoryTranslationsAsync(database, targetLocales, token);
        var seoTargetLocales = await AutomationCandidateCounter.GetSeoCandidateLocalesAsync(database, targetLocales, token);
        var completedSiteLocaleSets = await database.AutomationJobs
            .AsNoTracking()
            .Where(job => job.Type == AutomationJobType.SiteLocalization && job.Status == AutomationJobStatus.Completed)
            .Select(job => job.TargetLocales)
            .ToListAsync(token);
        var completedSiteLocales = completedSiteLocaleSets.SelectMany(locales => locales)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var pendingSiteLocales = activeLocales.Count(locale => locale != "tr-TR" && !completedSiteLocales.Contains(locale));

        return Results.Ok(new
        {
            activeLocales,
            publishedArticles,
            missingTranslations,
            seoCandidates,
            siteLanguageCandidates = pendingSiteLocales,
            reportCandidates = 0,
            runnerEnabled = configuration.GetValue<bool>("Automation:RunnerEnabled"),
            workloads = new
            {
                contentTranslation = new { count = missingTranslations, targetLocales, blockedReason = missingTranslations == 0 ? "Çevrilecek eksik içerik bulunmuyor." : null },
                categoryLocalization = new { count = categoryCandidates, targetLocales, blockedReason = categoryCandidates == 0 ? "Eksik kategori çevirisi bulunmuyor." : null },
                seoLocalization = new { count = seoCandidates, targetLocales = seoTargetLocales, blockedReason = seoCandidates == 0 && missingTranslations > 0 ? "Önce hedef dillerde içerik çeviri taslakları oluşturulmalıdır." : seoCandidates == 0 ? "SEO yerelleştirmesi bekleyen hedef dil kaydı bulunmuyor." : null },
                siteLocalization = new { count = pendingSiteLocales, targetLocales = targetLocales.Where(locale => !completedSiteLocales.Contains(locale)).ToArray(), blockedReason = pendingSiteLocales == 0 ? "Eksik site dili bulunmuyor." : null },
                systemReport = new { count = 0, targetLocales = Array.Empty<string>(), blockedReason = (string?)null }
            }
        });
    }

    private static async Task<IResult> CreateAsync(
        CreateRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> users,
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        if (!configuration.GetValue<bool>("Automation:RunnerEnabled"))
        {
            return Results.Conflict(new { message = "Codex worker etkinleştirilmeden toplu iş başlatılamaz." });
        }

        if (!Enum.TryParse<AutomationJobType>(request.Type, true, out var type))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["type"] = ["Desteklenmeyen toplu iş türü."]
            });
        }
        if (type == AutomationJobType.ReadyContentGeneration)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["type"] = ["Hazır içerik işleri özel oluşturma ekranından başlatılmalıdır."] });

        var actor = await users.GetUserAsync(principal);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        var activeLocales = await database.Locales
            .AsNoTracking()
            .Where(locale => locale.IsEnabled)
            .Select(locale => locale.Code)
            .ToArrayAsync(token);
        var requestedLocales = request.TargetLocales
            .Where(activeLocales.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (type is not AutomationJobType.SystemReport && requestedLocales.Length == 0)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["targetLocales"] = ["En az bir etkin hedef dil gereklidir."]
            });
        }

        var duplicateExists = await database.AutomationJobs.AnyAsync(job =>
            job.Type == type &&
            (job.Status == AutomationJobStatus.Queued ||
             job.Status == AutomationJobStatus.Running ||
             job.Status == AutomationJobStatus.Paused), token);
        if (duplicateExists)
        {
            return Results.Conflict(new { message = "Bu tür için zaten etkin bir toplu iş var." });
        }

        var totalItems = await CountItemsAsync(type, requestedLocales, database, token);
        if (totalItems == 0)
        {
            var message = type == AutomationJobType.SeoLocalization
                ? "SEO yerelleştirmesi için önce hedef dillerde içerik çeviri taslakları oluşturulmalıdır."
                : "Bu iş türü için işlenecek eksik kayıt bulunamadı.";
            return Results.Conflict(new { message });
        }

        var job = new AutomationJob(type, requestedLocales, totalItems, actor.Id, DateTimeOffset.UtcNow);
        database.AutomationJobs.Add(job);
        await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/automation/{job.Id}", new { job.Id });
    }

    private static async Task<IResult> GetAutomaticContentAsync(PublishingDbContext database, CancellationToken token)
    {
        var schedule = await database.AutomaticContentSchedules.AsNoTracking().SingleOrDefaultAsync(token);
        return Results.Ok(new { isEnabled = schedule?.IsEnabled ?? false, intervalMinutes = schedule?.IntervalMinutes ?? 3,
            schedule?.NextRunAt, schedule?.LastEnqueuedAt, schedule?.LastJobId });
    }

    private static async Task<IResult> UpdateAutomaticContentAsync(AutomaticContentRequest request,
        System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users,
        PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (request.IsEnabled && !configuration.GetValue<bool>("Automation:RunnerEnabled"))
            return Results.Conflict(new { message = "Codex worker etkin değil." });
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var now = DateTimeOffset.UtcNow;
        var schedule = await database.AutomaticContentSchedules.SingleOrDefaultAsync(token);
        if (schedule is null) { schedule = new AutomaticContentSchedule(actor.Id, now); database.AutomaticContentSchedules.Add(schedule); }
        schedule.SetEnabled(request.IsEnabled, actor.Id, now);
        await database.SaveChangesAsync(token);
        return Results.Ok(new { schedule.IsEnabled, schedule.IntervalMinutes, schedule.NextRunAt, schedule.LastEnqueuedAt, schedule.LastJobId });
    }

    private static async Task<IResult> CreateReadyContentAsync(
        ReadyContentRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> users,
        PublishingDbContext database,
        IConfiguration configuration,
        CancellationToken token)
    {
        if (!configuration.GetValue<bool>("Automation:RunnerEnabled")) return Results.Conflict(new { message = "Codex worker etkin değil." });
        if (request.Count is < 1 or > 50 || !Enum.TryParse<ArticleType>(request.ArticleType, true, out var articleType))
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = ["Geçerli tür ve 1-50 arasında adet gereklidir."] });
        var actor = await users.GetUserAsync(principal); if (actor is null) return Results.Unauthorized();
        var category = await database.Categories.AsNoTracking().Include(item => item.Locale)
            .SingleOrDefaultAsync(item => item.Id == request.CategoryId && item.Locale.IsDefault, token);
        if (category is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["categoryId"] = ["Geçerli bir Türkçe kategori gereklidir."] });
        var duplicate = await database.AutomationJobs.AnyAsync(job => job.Type == AutomationJobType.ReadyContentGeneration &&
            (job.Status == AutomationJobStatus.Queued || job.Status == AutomationJobStatus.Running || job.Status == AutomationJobStatus.Paused), token);
        if (duplicate) return Results.Conflict(new { message = "Zaten etkin bir hazır içerik üretim işi var." });
        var targetLocales = request.AutoTranslate
            ? await database.Locales.AsNoTracking().Where(locale => locale.IsEnabled && !locale.IsDefault).Select(locale => locale.Code).Order().ToArrayAsync(token)
            : [];
        var job = new AutomationJob(AutomationJobType.ReadyContentGeneration, targetLocales, request.Count, actor.Id, DateTimeOffset.UtcNow);
        job.ConfigureContentGeneration(category.Id, articleType.ToString(), request.IncludeImages, request.AutoTranslate, request.AutoSeo);
        database.AutomationJobs.Add(job); await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/automation/{job.Id}", new { job.Id });
    }

    private static async Task<int> CountItemsAsync(
        AutomationJobType type,
        string[] targetLocales,
        PublishingDbContext database,
        CancellationToken token)
    {
        if (type == AutomationJobType.SystemReport)
        {
            return 1;
        }

        if (type == AutomationJobType.SiteLocalization)
        {
            return targetLocales.Length;
        }

        if (type == AutomationJobType.SeoLocalization)
        {
            return await AutomationCandidateCounter.CountSeoCandidatesAsync(database, targetLocales, token);
        }

        if (type == AutomationJobType.CategoryLocalization)
        {
            return await AutomationCandidateCounter.CountMissingCategoryTranslationsAsync(database, targetLocales, token);
        }

        return await AutomationCandidateCounter.CountMissingTranslationsAsync(database, targetLocales, token);
    }

    private static async Task<IResult> ChangeStateAsync(
        Guid id,
        string action,
        PublishingDbContext database,
        CancellationToken token)
    {
        var job = await database.AutomationJobs.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (job is null)
        {
            return Results.NotFound();
        }

        try
        {
            var now = DateTimeOffset.UtcNow;
            switch (action.ToLowerInvariant())
            {
                case "pause": job.Pause(now); break;
                case "resume": job.Resume(now); break;
                case "retry": job.Retry(now); break;
                case "cancel": job.Cancel(now); break;
                default: return Results.NotFound();
            }
        }
        catch (InvalidOperationException exception)
        {
            return Results.Conflict(new { message = exception.Message });
        }

        await database.SaveChangesAsync(token);
        return Results.Ok(new { job.Id, status = job.Status.ToString() });
    }

    private sealed record CreateRequest(string Type, string[] TargetLocales);
    private sealed record ReadyContentRequest(Guid CategoryId, string ArticleType, int Count, bool IncludeImages, bool AutoTranslate, bool AutoSeo);
    private sealed record AutomaticContentRequest(bool IsEnabled);
}
