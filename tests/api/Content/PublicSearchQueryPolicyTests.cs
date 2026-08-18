using Peletnapechkai.Api.Features.Search;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class PublicSearchQueryPolicyTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("  yapay zekâ  ", "yapay zekâ")]
    public void Normalize_trims_meaningful_queries_and_rejects_blank_input(string? query, string? expected)
    {
        Assert.Equal(expected, PublicSearchQueryPolicy.Normalize(query));
    }

    [Fact]
    public void Query_length_contract_has_a_safe_and_usable_range()
    {
        Assert.Equal(2, PublicSearchQueryPolicy.MinimumLength);
        Assert.Equal(120, PublicSearchQueryPolicy.MaximumLength);
    }
}
