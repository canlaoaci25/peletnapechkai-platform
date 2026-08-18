using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class SourceEditorialReviewTests
{
    [Fact]
    public void Review_records_classification_and_timestamp()
    {
        var source = new Source("TÜBİTAK", new Uri("https://tubitak.gov.tr/"), DateTimeOffset.UtcNow.AddDays(-2));
        var reviewedAt = DateTimeOffset.UtcNow;
        source.Review(SourceKind.OfficialInstitution, reviewedAt);
        Assert.Equal(SourceKind.OfficialInstitution, source.Kind);
        Assert.Equal(reviewedAt, source.LastReviewedAt);
    }

    [Theory]
    [InlineData(SourceKind.Unclassified)]
    [InlineData((SourceKind)999)]
    public void Review_rejects_unclassified_or_unknown_values(SourceKind kind)
    {
        var source = new Source("Example", new Uri("https://example.com/"), DateTimeOffset.UtcNow);
        Assert.Throws<ArgumentOutOfRangeException>(() => source.Review(kind, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Review_state_distinguishes_current_due_and_unclassified_sources()
    {
        var now = new DateTimeOffset(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);
        Assert.Equal(SourceReviewState.Current, SourceTrustPolicy.GetReviewState(SourceKind.PrimaryResearch, now.AddDays(-364), now));
        Assert.Equal(SourceReviewState.ReviewDue, SourceTrustPolicy.GetReviewState(SourceKind.PrimaryResearch, now.AddDays(-366), now));
        Assert.Equal(SourceReviewState.Unclassified, SourceTrustPolicy.GetReviewState(SourceKind.Unclassified, now, now));
        Assert.Equal(SourceReviewState.Unclassified, SourceTrustPolicy.GetReviewState(SourceKind.NewsPublication, null, now));
    }
}
