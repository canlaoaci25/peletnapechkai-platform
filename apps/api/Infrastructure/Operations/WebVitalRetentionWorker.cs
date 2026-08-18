using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Infrastructure.Operations;

public sealed class WebVitalRetentionWorker(IServiceScopeFactory scopes, TimeProvider clock, ILogger<WebVitalRetentionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24), clock);
        do
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<PublishingDbContext>();
                var removed = await db.WebVitalSamples.Where(x => x.MeasuredAt < clock.GetUtcNow().AddDays(-90)).ExecuteDeleteAsync(stoppingToken);
                logger.LogInformation("Web Vitals retention removed {RemovedCount} expired samples.", removed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { logger.LogError(exception, "Web Vitals retention failed."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
