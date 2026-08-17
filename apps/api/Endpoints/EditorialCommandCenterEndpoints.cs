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
        var items = taskItems.Concat(reviewItems).OrderByDescending(item => item.IsMine)
            .ThenByDescending(item => EditorialCommandPriority.Score(item.Kind, item.Priority))
            .ThenBy(item => item.DueAt).Take(60).ToArray();
        return Results.Ok(new { checkedAt = now, summary = new {
            overdue = taskItems.Count(item => item.Kind == "OverdueTask"),
            dueSoon = taskItems.Count(item => item.Kind == "Task" && item.DueAt <= now.AddDays(2)),
            inReview = reviewItems.Count, incompleteQuality, personalOpen, personalOverdue, personalDueSoon,
            unassigned, teamMembers = workloads.Length }, workloads, users = activeUsers, items });
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
}

public sealed record EditorialCommandItem(Guid ArticleId, string Title, string Locale, string Kind,
    DateTimeOffset DueAt, string? TaskTitle, string? Assignee, Guid? AssigneeUserId, string? Priority, Guid? TaskId, string? Status, bool IsMine);
public sealed record EditorialWorkload(Guid UserId, string DisplayName, int Open, int Overdue, int DueSoon);
public sealed record ReassignRequest(Guid AssigneeUserId);

public static class EditorialCommandPriority
{
    public static int Score(string kind, string? priority) => kind switch {
        "OverdueTask" when priority == "Urgent" => 500, "OverdueTask" => 400,
        "Task" when priority == "Urgent" => 300, "EditorialReview" => 220,
        "SeoReview" => 210, "Task" => 100, _ => 0 };
}
