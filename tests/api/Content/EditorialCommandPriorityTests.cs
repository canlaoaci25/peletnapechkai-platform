using Peletnapechkai.Api.Endpoints;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class EditorialCommandPriorityTests
{
    [Theory]
    [InlineData("OverdueTask", "Urgent", "EditorialReview", null)]
    [InlineData("OverdueTask", "Normal", "Task", "Urgent")]
    [InlineData("EditorialReview", null, "SeoReview", null)]
    [InlineData("SeoReview", null, "QualityGate", null)]
    [InlineData("QualityGate", null, "Task", "Normal")]
    public void Higher_risk_work_is_ranked_first(string firstKind, string? firstPriority, string secondKind, string? secondPriority)
    {
        Assert.True(EditorialCommandPriority.Score(firstKind, firstPriority) > EditorialCommandPriority.Score(secondKind, secondPriority));
    }

    [Fact]
    public void Quality_debt_reports_only_missing_gates_in_editorial_order()
    {
        Assert.Equal(["SourcesVerified", "SeoMetadata", "CoverAccessibility"],
            EditorialQualityDebt.Missing(true, false, true, false, false, true, true, true));
    }

    [Fact]
    public void Reassignment_changes_owner_and_update_time()
    {
        var now = DateTimeOffset.UtcNow;
        var region = new Region(Guid.NewGuid(), "TR", "Türkiye", "TRY");
        var locale = new Locale(Guid.NewGuid(), "tr-TR", "tr", region, "Türkçe", "Türkçe", true);
        var article = new ArticleLocalization(new ArticleGroup(ArticleType.Analysis, now), locale, "test", "Test", "Özet", "Gövde", now);
        var task = new EditorialTask(article, Guid.NewGuid(), "Source review", EditorialTaskPriority.High, now.AddDays(1), Guid.NewGuid(), now);
        var nextOwner = Guid.NewGuid();
        task.Reassign(nextOwner, now.AddMinutes(5));
        Assert.Equal(nextOwner, task.AssigneeUserId);
        Assert.Equal(now.AddMinutes(5), task.UpdatedAt);
    }

    [Fact]
    public void Bulk_assignment_rejects_duplicates_empty_ids_and_oversized_requests()
    {
        var id = Guid.NewGuid();
        Assert.Null(EditorialBulkAssignment.Normalize([id, id]));
        Assert.Null(EditorialBulkAssignment.Normalize([Guid.Empty]));
        Assert.Null(EditorialBulkAssignment.Normalize(Enumerable.Range(0, 26).Select(_ => Guid.NewGuid()).ToArray()));
        Assert.Equal([id], EditorialBulkAssignment.Normalize([id])!);
    }

    [Fact]
    public void Bulk_assignment_audit_round_trips_server_trusted_undo_state()
    {
        var batchId=Guid.NewGuid();var taskId=Guid.NewGuid();var previous=Guid.NewGuid();var next=Guid.NewGuid();
        var log=new Peletnapechkai.Api.Domain.Auditing.AuditLog(Guid.NewGuid(),"editorial.task_bulk_reassigned",nameof(EditorialTask),taskId,
            System.Text.Json.JsonSerializer.Serialize(new{batchId,previousAssigneeUserId=previous,assigneeUserId=next}),DateTimeOffset.UtcNow);
        Assert.Equal(new BulkAssignmentAudit(batchId,taskId,previous,next),EditorialBulkAssignment.Read(log));
    }

    [Fact]
    public void Freshness_policy_explains_content_and_source_risk_without_hiding_the_cause()
    {
        var now = new DateTimeOffset(2026, 8, 17, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(["ContentOverOneYear", "SourcesUnreviewed"],
            EditorialFreshnessPolicy.Assess(now, now.AddDays(-400), 2, 1, null));
        Assert.Equal(["ContentOverSixMonths", "SourcesReviewStale"],
            EditorialFreshnessPolicy.Assess(now, now.AddDays(-200), 2, 0, now.AddDays(-190)));
        Assert.Empty(EditorialFreshnessPolicy.Assess(now, now.AddDays(-20), 1, 0, now.AddDays(-20)));
    }

    [Fact]
    public void Schedule_policy_flags_same_day_locale_and_category_pressure()
    {
        var day = new DateTimeOffset(2026, 8, 20, 9, 0, 0, TimeSpan.Zero);
        var first = new EditorialScheduleRow(Guid.NewGuid(), "A", "tr-TR", day, ["Teknoloji", "Mobil"]);
        var second = new EditorialScheduleRow(Guid.NewGuid(), "B", "tr-TR", day.AddHours(4), ["Rehber", "Mobil"]);
        var third = new EditorialScheduleRow(Guid.NewGuid(), "C", "en-US", day.AddDays(1), ["Technology"]);

        var result = EditorialSchedulePolicy.Annotate([first, second, third], TimeZoneInfo.Utc);

        Assert.All(result.Take(2), item => Assert.Equal(["LocaleCollision", "CategoryCollision"], item.ConflictReasons));
        Assert.False(result[2].HasConflict);
    }

    [Fact]
    public void Schedule_policy_uses_editorial_timezone_at_utc_day_boundary()
    {
        var turkey = TimeZoneInfo.CreateCustomTimeZone("TR", TimeSpan.FromHours(3), "TR", "TR");
        var first = new EditorialScheduleRow(Guid.NewGuid(), "A", "tr-TR", new DateTimeOffset(2026, 8, 20, 22, 30, 0, TimeSpan.Zero), ["Bilim"]);
        var second = new EditorialScheduleRow(Guid.NewGuid(), "B", "en-US", new DateTimeOffset(2026, 8, 21, 8, 0, 0, TimeSpan.Zero), ["Bilim"]);

        var result = EditorialSchedulePolicy.Annotate([first, second], turkey);

        Assert.All(result, item => Assert.Contains("CategoryCollision", item.ConflictReasons));
    }
}
