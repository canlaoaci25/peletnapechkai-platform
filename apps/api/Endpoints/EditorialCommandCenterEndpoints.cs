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
        endpoints.MapPost("/api/v1/admin/editorial/freshness/{articleId:guid}/task", CreateFreshnessTaskAsync)
            .WithTags("Editorial").RequireAuthorization(AuthorizationPolicies.WriteContent).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> GetAsync(System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users,
        PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal);
        if (actor is null) return Results.Unauthorized();
        var now = DateTimeOffset.UtcNow;
        var completedRows = await database.EditorialTasks.AsNoTracking()
            .Where(task => task.CompletedAt != null && task.CompletedAt >= now.AddDays(-90))
            .Select(task => new EditorialPerformanceRow(task.CreatedAt, task.DueAt, task.CompletedAt!.Value))
            .ToListAsync(token);
        var unmeasuredCompleted = await database.EditorialTasks.AsNoTracking()
            .CountAsync(task => task.Status == EditorialTaskStatus.Completed && task.CompletedAt == null, token);
        var performance = EditorialPerformancePolicy.Build(now, completedRows, unmeasuredCompleted);
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
                OldestSourceReview = article.ArticleGroup.Sources.Min(source => (DateTimeOffset?)source.LastReviewedAt),
                Views = database.ArticleEngagements.Where(metric => metric.ArticleLocalizationId == article.Id)
                    .Select(metric => (long?)metric.ViewCount).FirstOrDefault(),
                SeoQualityOpen = database.ArticleQualityChecklists.Any(checklist =>
                    checklist.ArticleLocalizationId == article.Id && !checklist.SeoMetadata)
            }).ToListAsync(token);
        var freshnessItems = freshnessCandidates.Select(article => new {
                Article = article,
                Reasons = EditorialFreshnessPolicy.Assess(now, article.UpdatedAt, article.SourceCount,
                    article.UnreviewedSources, article.OldestSourceReview, article.Views, article.SeoQualityOpen)
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
        var scheduledRows = await database.ArticleLocalizations.AsNoTracking()
            .Where(article => article.Status == PublicationStatus.Scheduled && article.ScheduledAt >= now && article.ScheduledAt <= now.AddDays(14))
            .OrderBy(article => article.ScheduledAt)
            .Select(article => new EditorialScheduleRow(article.Id, article.Title, article.Locale.Code,
                article.ScheduledAt!.Value, article.Categories.OrderBy(category => category.Name).Select(category => category.Name).ToArray()))
            .ToListAsync(token);
        var schedule = EditorialSchedulePolicy.Annotate(scheduledRows, EditorialSchedulePolicy.PublishingTimeZone());
        var readyToSchedule = await database.ArticleLocalizations.AsNoTracking()
            .CountAsync(article => article.Status == PublicationStatus.InSeoReview, token);
        var items = taskItems.Concat(reviewItems).Concat(qualityItems).Concat(freshnessItems).OrderByDescending(item => item.IsMine)
            .ThenByDescending(item => EditorialCommandPriority.Score(item.Kind, item.Priority))
            .ThenBy(item => item.DueAt).Take(60).ToArray();
        return Results.Ok(new { checkedAt = now, summary = new {
            overdue = taskItems.Count(item => item.Kind == "OverdueTask"),
            dueSoon = taskItems.Count(item => item.Kind == "Task" && item.DueAt <= now.AddDays(2)),
            inReview = reviewItems.Count, incompleteQuality, personalOpen, personalOverdue, personalDueSoon,
            freshnessDebt = freshnessItems.Count, unassigned, teamMembers = workloads.Length,
            scheduled = schedule.Length, scheduleConflicts = schedule.Count(item => item.HasConflict), readyToSchedule },
            performance, schedule, workloads, users = activeUsers, items });
    }

    private static async Task<IResult> CreateFreshnessTaskAsync(Guid articleId,
        System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users,
        PublishingDbContext database, CancellationToken token)
    {
        var actor = await users.GetUserAsync(principal);
        if (actor is null) return Results.Unauthorized();
        var article = await database.ArticleLocalizations.Include(item => item.Locale)
            .SingleOrDefaultAsync(item => item.Id == articleId, token);
        if (article is null) return Results.NotFound();
        if (article.Status != PublicationStatus.Published || article.Locale.Code != "tr-TR")
            return Results.ValidationProblem(new Dictionary<string, string[]> {
                ["articleId"] = ["Freshness work must start from a published Turkish source edition."] });
        const string title = "Tazelik incelemesi: kaynak, SEO ve yeniden dağıtım kararı";
        var existing = await database.EditorialTasks.AsNoTracking().FirstOrDefaultAsync(task =>
            task.ArticleLocalizationId == articleId && task.Title == title && task.Status != EditorialTaskStatus.Completed, token);
        if (existing is not null) return Results.Ok(new { existing.Id, created = false });
        var now = DateTimeOffset.UtcNow;
        var task = new EditorialTask(article, actor.Id, title, EditorialTaskPriority.High, now.AddDays(7), actor.Id, now);
        database.EditorialTasks.Add(task);
        database.AuditLogs.Add(new AuditLog(actor.Id, "editorial.freshness_task_created", nameof(EditorialTask), task.Id,
            System.Text.Json.JsonSerializer.Serialize(new { articleId, sourceLocale = article.Locale.Code,
                policy = "content-source-seo-engagement-v1", redistributionRequiresEditorialCompletion = true }), now));
        await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/editorial/tasks/{task.Id}", new { task.Id, created = true });
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
public sealed record EditorialPerformanceRow(DateTimeOffset CreatedAt, DateTimeOffset DueAt, DateTimeOffset CompletedAt);
public sealed record EditorialThroughputWeek(DateTimeOffset StartsAt, int Completed);
public sealed record EditorialPerformanceWindow(int Days, int SampleSize, int OnTimePercent, double MedianHours, double P95Hours);
public sealed record EditorialPerformance(EditorialPerformanceWindow Last30Days, EditorialPerformanceWindow Last90Days,
    EditorialThroughputWeek[] WeeklyThroughput, int UnmeasuredCompleted);
public sealed record EditorialScheduleRow(Guid ArticleId, string Title, string Locale, DateTimeOffset ScheduledAt, string[] Categories);
public sealed record EditorialScheduleItem(Guid ArticleId, string Title, string Locale, DateTimeOffset ScheduledAt, string[] Categories, bool HasConflict, string[] ConflictReasons);
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

public static class EditorialPerformancePolicy
{
    public static EditorialPerformance Build(DateTimeOffset now, IReadOnlyCollection<EditorialPerformanceRow> measured,
        int unmeasuredCompleted = 0)
    {
        var valid = measured.Where(row => row.CompletedAt >= row.CreatedAt && row.CompletedAt <= now).ToArray();
        return new(Window(now, valid, 30), Window(now, valid, 90),
            Enumerable.Range(0, 13).Select(offset =>
            {
                var end = StartOfWeek(now).AddDays(-(12 - offset) * 7 + 7);
                var start = end.AddDays(-7);
                return new EditorialThroughputWeek(start, valid.Count(row => row.CompletedAt >= start && row.CompletedAt < end));
            }).ToArray(), Math.Max(0, unmeasuredCompleted));
    }

    private static EditorialPerformanceWindow Window(DateTimeOffset now, EditorialPerformanceRow[] rows, int days)
    {
        var window = rows.Where(row => row.CompletedAt >= now.AddDays(-days)).OrderBy(row => row.CompletedAt).ToArray();
        if (window.Length == 0) return new(days, 0, 0, 0, 0);
        var hours = window.Select(row => Math.Max(0, (row.CompletedAt - row.CreatedAt).TotalHours)).Order().ToArray();
        return new(days, window.Length,
            (int)Math.Round(window.Count(row => row.CompletedAt <= row.DueAt) * 100d / window.Length),
            Math.Round(Percentile(hours, .5), 1), Math.Round(Percentile(hours, .95), 1));
    }

    private static double Percentile(double[] sorted, double percentile)
    {
        if (sorted.Length == 1) return sorted[0];
        var position = (sorted.Length - 1) * percentile;
        var lower = (int)Math.Floor(position); var upper = (int)Math.Ceiling(position);
        return sorted[lower] + (sorted[upper] - sorted[lower]) * (position - lower);
    }

    private static DateTimeOffset StartOfWeek(DateTimeOffset value)
    {
        var daysSinceMonday = ((int)value.DayOfWeek + 6) % 7;
        return new DateTimeOffset(value.Year, value.Month, value.Day, 0, 0, 0, value.Offset).AddDays(-daysSinceMonday);
    }
}

public static class EditorialCommandPriority
{
    public static int Score(string kind, string? priority) => kind switch {
        "OverdueTask" when priority == "Urgent" => 500, "OverdueTask" => 400,
        "Task" when priority == "Urgent" => 300, "EditorialReview" => 220,
        "SeoReview" => 210, "QualityGate" => 200, "FreshnessDebt" => 190, "Task" => 100, _ => 0 };
}

public static class EditorialSchedulePolicy
{
    public static TimeZoneInfo PublishingTimeZone() => TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Turkey Standard Time" : "Europe/Istanbul");

    public static EditorialScheduleItem[] Annotate(IReadOnlyCollection<EditorialScheduleRow> rows, TimeZoneInfo timeZone)
    {
        return rows.OrderBy(item => item.ScheduledAt).Select(item => {
            var sameDay = rows.Where(other => other.ArticleId != item.ArticleId &&
                TimeZoneInfo.ConvertTime(other.ScheduledAt, timeZone).Date == TimeZoneInfo.ConvertTime(item.ScheduledAt, timeZone).Date).ToArray();
            var reasons = new List<string>(2);
            if (sameDay.Any(other => other.Locale == item.Locale)) reasons.Add("LocaleCollision");
            if (item.Categories.Any(category => sameDay.Any(other => other.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)))) reasons.Add("CategoryCollision");
            return new EditorialScheduleItem(item.ArticleId, item.Title, item.Locale, item.ScheduledAt,
                item.Categories, reasons.Count > 0, [.. reasons]);
        }).ToArray();
    }
}

public static class EditorialFreshnessPolicy
{
    public static string[] Assess(DateTimeOffset now, DateTimeOffset updatedAt, int sourceCount,
        int unreviewedSources, DateTimeOffset? oldestSourceReview, long? views = null, bool seoQualityOpen = false)
    {
        var reasons = new List<string>(3);
        if (updatedAt <= now.AddDays(-365)) reasons.Add("ContentOverOneYear");
        else if (updatedAt <= now.AddDays(-180)) reasons.Add("ContentOverSixMonths");
        if (sourceCount == 0 || unreviewedSources > 0) reasons.Add("SourcesUnreviewed");
        else if (oldestSourceReview <= now.AddDays(-180)) reasons.Add("SourcesReviewStale");
        if (reasons.Count > 0 || seoQualityOpen) {
            if (views is null) reasons.Add("TrafficEvidenceUnavailable");
            else if (views >= 100) reasons.Add("MeasuredReaderDemand");
        }
        if (seoQualityOpen) reasons.Add("SeoQualityOpen");
        return [.. reasons];
    }

    public static int Score(IEnumerable<string> reasons) => reasons.Sum(reason => reason switch {
        "ContentOverOneYear" => 4, "SourcesUnreviewed" => 3, "SourcesReviewStale" => 2,
        "MeasuredReaderDemand" => 3, "SeoQualityOpen" => 2, "ContentOverSixMonths" => 1,
        "TrafficEvidenceUnavailable" => 0, _ => 0
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
