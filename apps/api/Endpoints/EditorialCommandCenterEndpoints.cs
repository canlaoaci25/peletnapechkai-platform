using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Peletnapechkai.Api.Domain.Auditing;

namespace Peletnapechkai.Api.Endpoints;

public static class EditorialCommandCenterEndpoints
{
    public static IEndpointRouteBuilder MapEditorialCommandCenterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/admin/editorial/command-center", GetAsync)
            .WithTags("Editorial").RequireAuthorization(AuthorizationPolicies.WriteContent);
        endpoints.MapPost("/api/v1/admin/editorial/tasks/{taskId:guid}/assignee", ReassignAsync)
            .WithTags("Editorial").RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        endpoints.MapPost("/api/v1/admin/editorial/tasks/bulk-assignee", BulkReassignAsync)
            .WithTags("Editorial").RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        endpoints.MapPost("/api/v1/admin/editorial/tasks/bulk-assignee/undo", UndoBulkReassignAsync)
            .WithTags("Editorial").RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> GetAsync(System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users,
        PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal);
        if (actor is null) return Results.Unauthorized();
        var now = DateTimeOffset.UtcNow;
        var reviewItems = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Status == PublicationStatus.InEditorialReview || article.Status == PublicationStatus.InSeoReview)
            .OrderBy(article => article.UpdatedAt)
            .Select(article => new EditorialCommandItem(article.Id, article.Title, article.Locale.Code,
                article.Status == PublicationStatus.InEditorialReview ? "EditorialReview" : "SeoReview",
                article.UpdatedAt, null, null, null, null, null, null, false)).Take(24).ToListAsync(token);
        var openTasks = database.EditorialTasks.AsNoTracking().Where(task => task.Status != EditorialTaskStatus.Completed);
        var personalOverdue = await openTasks.CountAsync(task => task.AssigneeUserId == actor.Id && task.DueAt < now, token);
        var personalDueSoon = await openTasks.CountAsync(task => task.AssigneeUserId == actor.Id && task.DueAt >= now && task.DueAt <= now.AddDays(2), token);
        var personalOpen = await openTasks.CountAsync(task => task.AssigneeUserId == actor.Id, token);
        var taskItems = await database.EditorialTasks.AsNoTracking()
            .Where(task => task.Status != EditorialTaskStatus.Completed).OrderBy(task => task.DueAt)
            .Select(task => new EditorialCommandItem(task.ArticleLocalizationId, task.Article.Title, task.Article.Locale.Code,
                task.DueAt < now ? "OverdueTask" : "Task", task.DueAt, task.Title,
                database.Users.Where(user => user.Id == task.AssigneeUserId).Select(user => user.DisplayName).FirstOrDefault(),
                task.AssigneeUserId, task.Priority.ToString(), task.Id, task.Status.ToString(), task.AssigneeUserId == actor.Id)).Take(50).ToListAsync(token);
        var incompleteQuality = await database.ArticleQualityChecklists.AsNoTracking()
            .CountAsync(item => !item.TitleAndSummary || !item.SourcesVerified || !item.AuthorAndTaxonomy ||
                !item.SeoMetadata || !item.CoverAccessibility || !item.CommercialDisclosure ||
                !item.TranslationReviewed || !item.LegalEditorialReview, token);
        var qualityItems = await database.ArticleQualityChecklists.AsNoTracking()
            .Where(item => !item.TitleAndSummary || !item.SourcesVerified || !item.AuthorAndTaxonomy ||
                !item.SeoMetadata || !item.CoverAccessibility || !item.CommercialDisclosure ||
                !item.TranslationReviewed || !item.LegalEditorialReview)
            .OrderBy(item => item.Article.UpdatedAt)
            .Select(item => new EditorialCommandItem(item.ArticleLocalizationId, item.Article.Title, item.Article.Locale.Code,
                "QualityGate", item.UpdatedAt ?? item.Article.UpdatedAt, null, null, null, null, null, null, false,
                EditorialQualityDebt.Missing(item.TitleAndSummary, item.SourcesVerified, item.AuthorAndTaxonomy,
                    item.SeoMetadata, item.CoverAccessibility, item.CommercialDisclosure,
                    item.TranslationReviewed, item.LegalEditorialReview)))
            .Take(24).ToListAsync(token);
        var freshnessCandidates = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Status == PublicationStatus.Published)
            .OrderBy(article => article.UpdatedAt)
            .Take(200)
            .Select(article => new {
                article.Id, article.Title, Locale = article.Locale.Code, article.UpdatedAt,
                SourceCount = article.ArticleGroup.Sources.Count,
                UnreviewedSources = article.ArticleGroup.Sources.Count(source => source.LastReviewedAt == null),
                OldestSourceReview = article.ArticleGroup.Sources.Min(source => (DateTimeOffset?)source.LastReviewedAt)
            }).ToListAsync(token);
        var freshnessItems = freshnessCandidates.Select(article => new {
                Article = article,
                Reasons = EditorialFreshnessPolicy.Assess(now, article.UpdatedAt, article.SourceCount,
                    article.UnreviewedSources, article.OldestSourceReview)
            })
            .Where(item => item.Reasons.Length > 0)
            .OrderByDescending(item => EditorialFreshnessPolicy.Score(item.Reasons))
            .ThenBy(item => item.Article.UpdatedAt)
            .Take(24)
            .Select(item => new EditorialCommandItem(item.Article.Id, item.Article.Title, item.Article.Locale,
                "FreshnessDebt", item.Article.UpdatedAt, null, null, null, null, null, null, false,
                null, item.Reasons)).ToList();
        var activeUsers = await database.Users.AsNoTracking().Where(user => user.IsActive).OrderBy(user => user.DisplayName)
            .Select(user => new { user.Id, user.DisplayName }).ToListAsync(token);
        var workloadCounts = await openTasks.GroupBy(task => task.AssigneeUserId).Select(group => new {
            UserId = group.Key, Open = group.Count(), Overdue = group.Count(task => task.DueAt < now),
            DueSoon = group.Count(task => task.DueAt >= now && task.DueAt <= now.AddDays(2)) }).ToListAsync(token);
        var workloads = activeUsers.Select(user => {
            var counts = workloadCounts.FirstOrDefault(item => item.UserId == user.Id);
            return new EditorialWorkload(user.Id, user.DisplayName, counts?.Open ?? 0, counts?.Overdue ?? 0, counts?.DueSoon ?? 0);
        }).OrderByDescending(item => item.Overdue).ThenByDescending(item => item.Open).ThenBy(item => item.DisplayName).ToArray();
        var activeIds = activeUsers.Select(user => user.Id).ToHashSet();
        var unassigned = workloadCounts.Where(item => !activeIds.Contains(item.UserId)).Sum(item => item.Open);
        var items = taskItems.Concat(reviewItems).Concat(qualityItems).Concat(freshnessItems).OrderByDescending(item => item.IsMine)
            .ThenByDescending(item => EditorialCommandPriority.Score(item.Kind, item.Priority))
            .ThenBy(item => item.DueAt).Take(60).ToArray();
        return Results.Ok(new { checkedAt = now, summary = new {
            overdue = taskItems.Count(item => item.Kind == "OverdueTask"),
            dueSoon = taskItems.Count(item => item.Kind == "Task" && item.DueAt <= now.AddDays(2)),
            inReview = reviewItems.Count, incompleteQuality, personalOpen, personalOverdue, personalDueSoon,
            freshnessDebt = freshnessItems.Count, unassigned, teamMembers = workloads.Length }, workloads, users = activeUsers, items });
    }

    private static async Task<IResult> ReassignAsync(Guid taskId, ReassignRequest request,
        System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal);
        if (actor is null) return Results.Unauthorized();
        var task = await database.EditorialTasks.SingleOrDefaultAsync(item => item.Id == taskId, token);
        if (task is null) return Results.NotFound();
        var assignee = await database.Users.SingleOrDefaultAsync(user => user.Id == request.AssigneeUserId && user.IsActive, token);
        if (assignee is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["assigneeUserId"] = ["An active assignee is required."] });
        var previousAssigneeUserId = task.AssigneeUserId;
        if (previousAssigneeUserId == assignee.Id) return Results.Ok();
        var now = DateTimeOffset.UtcNow;
        task.Reassign(assignee.Id, now);
        database.AuditLogs.Add(new AuditLog(actor.Id, "editorial.task_reassigned", nameof(EditorialTask), task.Id,
            System.Text.Json.JsonSerializer.Serialize(new { task.ArticleLocalizationId, previousAssigneeUserId, assigneeUserId = assignee.Id }), now));
        await database.SaveChangesAsync(token);
        return Results.Ok();
    }

    private static async Task<IResult> BulkReassignAsync(BulkReassignRequest request,
        System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal);
        if (actor is null) return Results.Unauthorized();
        var taskIds = EditorialBulkAssignment.Normalize(request.TaskIds);
        if (taskIds is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["taskIds"] = ["Select between 1 and 25 unique tasks."] });
        var assignee = await database.Users.SingleOrDefaultAsync(user => user.Id == request.AssigneeUserId && user.IsActive, token);
        if (assignee is null) return Results.ValidationProblem(new Dictionary<string, string[]> { ["assigneeUserId"] = ["An active assignee is required."] });
        var tasks = await database.EditorialTasks.Where(item => taskIds.Contains(item.Id) && item.Status != EditorialTaskStatus.Completed).ToListAsync(token);
        if (tasks.Count != taskIds.Length) return Results.Conflict(new { error = "One or more tasks are missing or no longer open. Refresh the queue." });
        var now = DateTimeOffset.UtcNow;
        var batchId = Guid.CreateVersion7();
        var changed = 0;
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        foreach (var task in tasks)
        {
            var previousAssigneeUserId = task.AssigneeUserId;
            if (previousAssigneeUserId == assignee.Id) continue;
            task.Reassign(assignee.Id, now);
            database.AuditLogs.Add(new AuditLog(actor.Id, "editorial.task_bulk_reassigned", nameof(EditorialTask), task.Id,
                System.Text.Json.JsonSerializer.Serialize(new { batchId, task.ArticleLocalizationId, previousAssigneeUserId, assigneeUserId = assignee.Id }), now));
            changed++;
        }
        await database.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return Results.Ok(new { batchId = changed > 0 ? batchId : (Guid?)null, changed, assignee = assignee.DisplayName, undoUntil = changed > 0 ? now.AddMinutes(10) : (DateTimeOffset?)null });
    }

    private static async Task<IResult> UndoBulkReassignAsync(UndoBulkReassignRequest request,
        System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal);
        if (actor is null) return Results.Unauthorized();
        var now = DateTimeOffset.UtcNow;
        var recent = await database.AuditLogs.AsNoTracking()
            .Where(log => log.ActorUserId == actor.Id && log.Action == "editorial.task_bulk_reassigned" && log.OccurredAt >= now.AddMinutes(-10))
            .OrderByDescending(log => log.OccurredAt).Take(100).ToListAsync(token);
        var assignments = recent.Select(EditorialBulkAssignment.Read).Where(item => item is not null && item.BatchId == request.BatchId).Cast<BulkAssignmentAudit>().ToArray();
        if (assignments.Length == 0) return Results.Conflict(new { error = "This reassignment can no longer be undone." });
        var taskIds = assignments.Select(item => item.TaskId).ToArray();
        var tasks = await database.EditorialTasks.Where(task => taskIds.Contains(task.Id)).ToDictionaryAsync(task => task.Id, token);
        if (assignments.Any(item => !tasks.TryGetValue(item.TaskId, out var task) || task.AssigneeUserId != item.AssigneeUserId))
            return Results.Conflict(new { error = "A selected task changed after this reassignment. Nothing was undone." });
        await using var transaction = await database.Database.BeginTransactionAsync(token);
        foreach (var assignment in assignments)
        {
            var task = tasks[assignment.TaskId];
            task.Reassign(assignment.PreviousAssigneeUserId, now);
            database.AuditLogs.Add(new AuditLog(actor.Id, "editorial.task_bulk_reassignment_undone", nameof(EditorialTask), task.Id,
                System.Text.Json.JsonSerializer.Serialize(new { request.BatchId, assignment.AssigneeUserId, restoredAssigneeUserId = assignment.PreviousAssigneeUserId }), now));
        }
        await database.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return Results.Ok(new { restored = assignments.Length });
    }
}

public sealed record EditorialCommandItem(Guid ArticleId, string Title, string Locale, string Kind,
    DateTimeOffset DueAt, string? TaskTitle, string? Assignee, Guid? AssigneeUserId, string? Priority, Guid? TaskId, string? Status, bool IsMine,
    string[]? MissingGates = null, string[]? FreshnessReasons = null);
public sealed record EditorialWorkload(Guid UserId, string DisplayName, int Open, int Overdue, int DueSoon);
public sealed record ReassignRequest(Guid AssigneeUserId);
public sealed record BulkReassignRequest(Guid[] TaskIds, Guid AssigneeUserId);
public sealed record UndoBulkReassignRequest(Guid BatchId);
public sealed record BulkAssignmentAudit(Guid BatchId, Guid TaskId, Guid PreviousAssigneeUserId, Guid AssigneeUserId);

public static class EditorialBulkAssignment
{
    public static Guid[]? Normalize(Guid[]? taskIds)
    {
        if (taskIds is null) return null;
        var normalized = taskIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        return normalized.Length is >= 1 and <= 25 && normalized.Length == taskIds.Length ? normalized : null;
    }

    public static BulkAssignmentAudit? Read(AuditLog log)
    {
        if (string.IsNullOrWhiteSpace(log.DetailsJson)) return null;
        try
        {
            using var json = System.Text.Json.JsonDocument.Parse(log.DetailsJson);
            var root = json.RootElement;
            return new(root.GetProperty("batchId").GetGuid(), log.EntityId,
                root.GetProperty("previousAssigneeUserId").GetGuid(), root.GetProperty("assigneeUserId").GetGuid());
        }
        catch (System.Text.Json.JsonException) { return null; }
        catch (KeyNotFoundException) { return null; }
        catch (InvalidOperationException) { return null; }
    }
}

public static class EditorialCommandPriority
{
    public static int Score(string kind, string? priority) => kind switch {
        "OverdueTask" when priority == "Urgent" => 500, "OverdueTask" => 400,
        "Task" when priority == "Urgent" => 300, "EditorialReview" => 220,
        "SeoReview" => 210, "QualityGate" => 200, "FreshnessDebt" => 190, "Task" => 100, _ => 0 };
}

public static class EditorialFreshnessPolicy
{
    public static string[] Assess(DateTimeOffset now, DateTimeOffset updatedAt, int sourceCount,
        int unreviewedSources, DateTimeOffset? oldestSourceReview)
    {
        var reasons = new List<string>(3);
        if (updatedAt <= now.AddDays(-365)) reasons.Add("ContentOverOneYear");
        else if (updatedAt <= now.AddDays(-180)) reasons.Add("ContentOverSixMonths");
        if (sourceCount == 0 || unreviewedSources > 0) reasons.Add("SourcesUnreviewed");
        else if (oldestSourceReview <= now.AddDays(-180)) reasons.Add("SourcesReviewStale");
        return [.. reasons];
    }

    public static int Score(IEnumerable<string> reasons) => reasons.Sum(reason => reason switch {
        "ContentOverOneYear" => 4, "SourcesUnreviewed" => 3, "SourcesReviewStale" => 2,
        "ContentOverSixMonths" => 1, _ => 0
    });
}

public static class EditorialQualityDebt
{
    public static string[] Missing(bool titleAndSummary, bool sourcesVerified, bool authorAndTaxonomy,
        bool seoMetadata, bool coverAccessibility, bool commercialDisclosure, bool translationReviewed, bool legalEditorialReview)
    {
        var gates = new List<string>(8);
        if (!titleAndSummary) gates.Add("TitleAndSummary");
        if (!sourcesVerified) gates.Add("SourcesVerified");
        if (!authorAndTaxonomy) gates.Add("AuthorAndTaxonomy");
        if (!seoMetadata) gates.Add("SeoMetadata");
        if (!coverAccessibility) gates.Add("CoverAccessibility");
        if (!commercialDisclosure) gates.Add("CommercialDisclosure");
        if (!translationReviewed) gates.Add("TranslationReviewed");
        if (!legalEditorialReview) gates.Add("LegalEditorialReview");
        return [.. gates];
    }
}
