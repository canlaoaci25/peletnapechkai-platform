using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class ContentAuthorityPolicyTests
{
    [Fact]
    public void Complete_independent_evidence_receives_full_score()
    {
        var result = ContentAuthorityPolicy.Assess(["https://www.tuik.gov.tr/report", "https://www.who.int/research"], true, true, 1, 2);
        Assert.Equal(100, result.Score);
        Assert.Empty(result.Risks);
    }

    [Fact]
    public void Missing_and_low_diversity_evidence_is_explained()
    {
        var result = ContentAuthorityPolicy.Assess(["http://example.com/a", "http://example.com/b"], false, false, 0, 0);
        Assert.Equal(40, result.Score);
        Assert.Equal(["single_domain", "insecure_source", "missing_seo", "missing_cover", "missing_category", "missing_tags"], result.Risks);
    }

    [Fact]
    public void No_sources_is_a_material_risk()
    {
        var result = ContentAuthorityPolicy.Assess([], true, true, 1, 1);
        Assert.Equal(65, result.Score);
        Assert.Equal(["missing_sources"], result.Risks);
    }
}
