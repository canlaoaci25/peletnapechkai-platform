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
}
