using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
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
        group.MapGet("/scan", ScanAsync);
        group.MapPost("/", CreateAsync).ValidateAntiforgery();
        group.MapPost("/{id:guid}/{action}", ChangeStateAsync).ValidateAntiforgery();
        return endpoints;
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
            })
            .ToListAsync(token));

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
        var publishedGroups = await database.ArticleLocalizations
            .AsNoTracking()
            .Where(article => article.Status == PublicationStatus.Published)
            .Select(article => article.ArticleGroupId)
            .Distinct()
            .CountAsync(token);
        var publishedLocalePairs = await database.ArticleLocalizations
            .AsNoTracking()
            .Where(article => article.Status == PublicationStatus.Published && article.Locale.IsEnabled)
            .CountAsync(token);
        var seoCandidates = await database.ArticleLocalizations
            .AsNoTracking()
            .CountAsync(article => article.Status == PublicationStatus.Published &&
                (article.SeoTitle == null || article.SeoDescription == null), token);

        return Results.Ok(new
        {
            activeLocales,
            publishedArticles,
            missingTranslations = Math.Max(0, publishedGroups * activeLocales.Length - publishedLocalePairs),
            seoCandidates,
            siteLanguageCandidates = Math.Max(0, activeLocales.Length - 1),
            reportCandidates = 1,
            runnerEnabled = configuration.GetValue<bool>("Automation:RunnerEnabled")
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
            return Results.Conflict(new { message = "Bu iş türü için işlenecek eksik kayıt bulunamadı." });
        }

        var job = new AutomationJob(type, requestedLocales, totalItems, actor.Id, DateTimeOffset.UtcNow);
        database.AutomationJobs.Add(job);
        await database.SaveChangesAsync(token);
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
            return await database.ArticleLocalizations.CountAsync(article =>
                article.Status == PublicationStatus.Published &&
                targetLocales.Contains(article.Locale.Code) &&
                (article.SeoTitle == null || article.SeoDescription == null), token);
        }

        var publishedGroups = await database.ArticleLocalizations
            .Where(article => article.Status == PublicationStatus.Published)
            .Select(article => article.ArticleGroupId)
            .Distinct()
            .CountAsync(token);
        var existingPairs = await database.ArticleLocalizations.CountAsync(article =>
            article.Status == PublicationStatus.Published && targetLocales.Contains(article.Locale.Code), token);
        return Math.Max(0, publishedGroups * targetLocales.Length - existingPairs);
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
}
