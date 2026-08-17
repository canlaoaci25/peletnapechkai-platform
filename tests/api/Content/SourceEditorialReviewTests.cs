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
}
