using Peletnapechkai.Api.Infrastructure.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class GeneratedSourceQualityPolicyTests
{
    [Fact]
    public void Independent_public_sources_are_accepted()
    {
        (string? Name, string? Url)[] sources =
        {
            ("TÜİK", "https://data.tuik.gov.tr/bulten"),
            ("Dünya Bankası", "https://www.worldbank.org/tr/country/turkey")
        };

        Assert.True(GeneratedSourceQualityPolicy.IsValid(sources));
    }

    [Theory]
    [MemberData(nameof(InvalidSources))]
    public void Unsafe_or_low_diversity_sources_are_rejected((string? Name, string? Url)[] sources)
    {
        Assert.False(GeneratedSourceQualityPolicy.IsValid(sources));
    }

    public static TheoryData<(string? Name, string? Url)[]> InvalidSources => new()
    {
        new (string?, string?)[] { ("A", "https://example.com/a"), ("B", "https://example.com/b") },
        new (string?, string?)[] { ("A", "https://example.com/report#one"), ("B", "https://example.com/report#two") },
        new (string?, string?)[] { ("Yerel", "http://localhost:5000/report"), ("B", "https://example.org/report") },
        new (string?, string?)[] { ("Özel ağ", "http://192.168.1.20/report"), ("B", "https://example.org/report") },
        new (string?, string?)[] { ("Eşlenmiş özel ağ", "http://[::ffff:192.168.1.20]/report"), ("B", "https://example.org/report") },
        new (string?, string?)[] { ("Kimlik bilgili", "https://user:pass@example.com/report"), ("B", "https://example.org/report") },
        new (string?, string?)[] { (null, "https://example.com/report"), ("B", "https://example.org/report") }
    };
}
