using System.Data;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static partial class AutomationWorkerEndpoints
{
    private static readonly TimeSpan VisualLeaseDuration = TimeSpan.FromMinutes(5);

    private static async Task<IResult> ClaimVisualTaskAsync(HttpContext context, PublishingDbContext database,
        IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var owner = context.Request.Headers["X-Worker-Id"].ToString().Trim();
        if (string.IsNullOrWhiteSpace(owner) || owner.Length > 120)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["X-Worker-Id"] = ["A worker identifier of at most 120 characters is required."] });

        var now = DateTimeOffset.UtcNow;
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
        var task = await database.VisualReviewTasks
            .Where(candidate => candidate.Status == VisualReviewStatus.Pending || candidate.Status == VisualReviewStatus.RetryRequested)
            .Where(candidate => candidate.DeadLetteredAt == null)
            .Where(candidate => candidate.NextAttemptAt == null || candidate.NextAttemptAt <= now)
            .Where(candidate => candidate.LeaseToken == null || candidate.LeaseExpiresAt <= now)
            .OrderBy(candidate => candidate.NextAttemptAt).ThenBy(candidate => candidate.QualityScore).ThenBy(candidate => candidate.CreatedAt)
            .FirstOrDefaultAsync(token);
        if (task is null) return Results.NoContent();
        var leaseToken = task.ClaimGeneration(owner, now, VisualLeaseDuration);
        try { await database.SaveChangesAsync(token); await transaction.CommitAsync(token); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict(new { code = "visual-lease-race" }); }
        return Results.Ok(new { task.Id, leaseToken, task.ArticleLocalizationId, task.Target, task.SectionHeading,
            task.SectionContext, task.VisualPurpose, task.ProposedPrompt, task.NegativePrompt, task.AttemptCount,
            leaseExpiresAt = task.LeaseExpiresAt });
    }

    private static async Task<IResult> HeartbeatVisualTaskAsync(Guid id, VisualLeaseRequest request, HttpContext context,
        PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var task = await database.VisualReviewTasks.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (task is null) return Results.NotFound();
        try { task.RenewGenerationLease(request.LeaseToken, DateTimeOffset.UtcNow, VisualLeaseDuration); }
        catch (InvalidOperationException error) { return Results.Conflict(new { code = "visual-lease-invalid", message = error.Message }); }
        await database.SaveChangesAsync(token);
        return Results.Ok(new { task.Id, task.LeaseExpiresAt });
    }

    private static async Task<IResult> FailVisualTaskAsync(Guid id, VisualFailureRequest request, HttpContext context,
        PublishingDbContext database, IConfiguration configuration, CancellationToken token)
    {
        if (!IsAuthorized(context, configuration)) return Results.Unauthorized();
        var task = await database.VisualReviewTasks.SingleOrDefaultAsync(candidate => candidate.Id == id, token);
        if (task is null) return Results.NotFound();
        try { task.RecordGenerationFailure(request.LeaseToken, request.FailureCode, DateTimeOffset.UtcNow); }
        catch (ArgumentException error) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["failureCode"] = [error.Message] }); }
        catch (InvalidOperationException error) { return Results.Conflict(new { code = "visual-lease-invalid", message = error.Message }); }
        await database.SaveChangesAsync(token);
        return Results.Ok(new { task.Id, status = task.Status.ToString(), task.AttemptCount, task.NextAttemptAt, task.LastFailureCode });
    }

    private sealed record VisualLeaseRequest(Guid LeaseToken);
    private sealed record VisualFailureRequest(Guid LeaseToken, string FailureCode);
}
