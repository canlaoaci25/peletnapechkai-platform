using Peletnapechkai.Api.Infrastructure.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class VisualSimilarityAnalyzerTests
{
    [Fact]
    public void Identical_hash_is_rejected_as_duplicate()
    {
        var id = Guid.NewGuid();
        var result = VisualSimilarityAnalyzer.Assess("AAAAAAAAAAAAAAAA", [(id, "AAAAAAAAAAAAAAAA")]);
        Assert.Equal(0, result.OriginalityScore);
        Assert.Equal(100, result.ClosestSimilarityPercent);
        Assert.Equal(id, result.ClosestMediaAssetId);
    }

    [Fact]
    public void Different_hashes_receive_measured_originality()
    {
        var result = VisualSimilarityAnalyzer.Assess("0000000000000000", [(Guid.NewGuid(), "FFFFFFFFFFFFFFFF")]);
        Assert.Equal(100, result.OriginalityScore);
        Assert.Equal(0, result.ClosestSimilarityPercent);
    }
}
