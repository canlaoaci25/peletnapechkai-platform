using Peletnapechkai.Api.Endpoints;

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
}
