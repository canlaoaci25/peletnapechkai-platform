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
    public void Higher_risk_work_is_ranked_first(string firstKind, string? firstPriority, string secondKind, string? secondPriority)
    {
        Assert.True(EditorialCommandPriority.Score(firstKind, firstPriority) > EditorialCommandPriority.Score(secondKind, secondPriority));
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
}
