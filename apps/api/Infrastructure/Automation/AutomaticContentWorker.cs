using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public sealed class AutomaticContentWorker(IServiceScopeFactory scopeFactory, TimeProvider timeProvider, IConfiguration configuration, ILogger<AutomaticContentWorker> logger) : BackgroundService
{
    private static readonly ArticleType[] Types = [ArticleType.News, ArticleType.Guide, ArticleType.Review, ArticleType.Analysis];
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await TryEnqueueAsync(stoppingToken);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15), timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken)) await TryEnqueueAsync(stoppingToken);
    }
    private async Task TryEnqueueAsync(CancellationToken token)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<PublishingDbContext>();
            var now = timeProvider.GetUtcNow();
            var schedule = await database.AutomaticContentSchedules.SingleOrDefaultAsync(token);
            if (schedule is null || !schedule.IsEnabled || schedule.NextRunAt > now) return;
            var busy = await database.AutomationJobs.AnyAsync(job => job.Type == AutomationJobType.ReadyContentGeneration &&
                (job.Status == AutomationJobStatus.Queued || job.Status == AutomationJobStatus.Running || job.Status == AutomationJobStatus.Paused), token);
            if (busy) return;
            var campaign=PriorityContentCampaign.Load(configuration,now);
            var categoryIds = campaign is null
                ?await database.Categories.AsNoTracking().Where(category => category.Locale.IsDefault).Select(category => category.Id).ToArrayAsync(token)
                :await database.Categories.AsNoTracking().Where(category => category.Locale.IsDefault&&category.Slug==campaign.CategorySlug).Select(category => category.Id).ToArrayAsync(token);
            if (categoryIds.Length == 0) return;
            var locales = await database.Locales.AsNoTracking().Where(locale => locale.IsEnabled && !locale.IsDefault).Select(locale => locale.Code).Order().ToArrayAsync(token);
            var categoryId = categoryIds[Random.Shared.Next(categoryIds.Length)];
            var type = Types[Random.Shared.Next(Types.Length)];
            var job = new AutomationJob(AutomationJobType.ReadyContentGeneration, locales, 1, schedule.UpdatedByUserId, now);
            job.ConfigureContentGeneration(categoryId, type.ToString(), includeImages: true, autoTranslate: true, autoSeo: true);
            job.MarkAutomaticallyScheduled();
            database.AutomationJobs.Add(job); schedule.MarkEnqueued(job.Id, now);
            await database.SaveChangesAsync(token);
            logger.LogInformation("Automatic content job {JobId} queued.", job.Id);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception exception) { logger.LogError(exception, "Automatic content scheduling cycle failed; retrying next cycle."); }
    }
}
