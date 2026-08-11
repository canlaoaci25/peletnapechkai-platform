using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public static class AutomationCandidateCounter
{
    public static async Task<int> CountReadyContentRemainingAsync(PublishingDbContext database, Peletnapechkai.Api.Domain.Automation.AutomationJob job, CancellationToken token)
    {
        var sources = await database.ArticleLocalizations.AsNoTracking().Where(article => article.GeneratedByAutomationJobId == job.Id && article.Locale.IsDefault && article.Status == PublicationStatus.Published).ToArrayAsync(token);
        var remaining = Math.Max(0, job.TotalItems - sources.Length);
        if (job.AutoTranslate && sources.Length > 0)
        {
            var groupIds = sources.Select(article => article.ArticleGroupId).ToArray();
            var translated = await database.ArticleLocalizations.AsNoTracking().CountAsync(article => article.GeneratedByAutomationJobId == job.Id && groupIds.Contains(article.ArticleGroupId) && !article.Locale.IsDefault && article.Status == PublicationStatus.Published, token);
            remaining += Math.Max(0, sources.Length * job.TargetLocales.Length - translated);
        }
        if (job.AutoSeo)
            remaining += await database.ArticleLocalizations.AsNoTracking().CountAsync(article => article.GeneratedByAutomationJobId == job.Id && article.Status == PublicationStatus.Published && (article.SeoTitle == null || article.SeoDescription == null), token);
        return remaining;
    }

    public static async Task<int> CountMissingTranslationsAsync(PublishingDbContext database, string[] targetLocales, CancellationToken token)
    {
        if (targetLocales.Length == 0) return 0;
        var sourceGroups = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Status == PublicationStatus.Published && article.Locale.IsDefault)
            .Select(article => article.ArticleGroupId).Distinct().ToArrayAsync(token);
        if (sourceGroups.Length == 0) return 0;
        var existingPairs = await database.ArticleLocalizations.AsNoTracking()
            .CountAsync(article => sourceGroups.Contains(article.ArticleGroupId) &&
                targetLocales.Contains(article.Locale.Code) && article.Status != PublicationStatus.Archived, token);
        return Math.Max(0, sourceGroups.Length * targetLocales.Length - existingPairs);
    }

    public static Task<int> CountSeoCandidatesAsync(PublishingDbContext database, string[] targetLocales, CancellationToken token) =>
        targetLocales.Length == 0 ? Task.FromResult(0) : database.ArticleLocalizations.AsNoTracking().CountAsync(article =>
            targetLocales.Contains(article.Locale.Code) &&
            article.Status == PublicationStatus.Published &&
            (article.SeoTitle == null || article.SeoDescription == null), token);

    public static async Task<string[]> GetSeoCandidateLocalesAsync(PublishingDbContext database, string[] targetLocales, CancellationToken token) =>
        await database.ArticleLocalizations.AsNoTracking()
            .Where(article => targetLocales.Contains(article.Locale.Code) &&
                article.Status == PublicationStatus.Published &&
                (article.SeoTitle == null || article.SeoDescription == null))
            .Select(article => article.Locale.Code).Distinct().Order().ToArrayAsync(token);
}
