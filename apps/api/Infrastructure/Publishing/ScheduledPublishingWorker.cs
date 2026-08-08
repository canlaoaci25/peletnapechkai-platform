using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Infrastructure.Publishing;

public sealed class ScheduledPublishingWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<ScheduledPublishingWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await PublishDueArticlesAsync(stoppingToken);
        using var timer = new PeriodicTimer(Interval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PublishDueArticlesAsync(stoppingToken);
        }
    }

    private async Task PublishDueArticlesAsync(CancellationToken token)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<PublishingDbContext>();
            var now = timeProvider.GetUtcNow();
            var dueArticles = await database.ArticleLocalizations
                .Where(article => article.Status == PublicationStatus.Scheduled && article.ScheduledAt <= now)
                .OrderBy(article => article.ScheduledAt)
                .Take(100)
                .ToListAsync(token);

            foreach (var article in dueArticles)
            {
                article.Publish(article.ScheduledAt ?? now);
                database.AuditLogs.Add(new AuditLog(null, "editorial.auto-published", nameof(ArticleLocalization), article.Id, null, now));
            }

            if (dueArticles.Count > 0)
            {
                await database.SaveChangesAsync(token);
                logger.LogInformation("Automatically published {ArticleCount} scheduled articles.", dueArticles.Count);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Scheduled publishing cycle failed; the next cycle will retry.");
        }
    }
}
