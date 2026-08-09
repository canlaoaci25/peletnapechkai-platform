using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Localization;

public sealed class LocaleCountryTests
{
    [Fact]
    public void Required_country_can_be_disabled_without_deleting_mapping()
    {
        var country = new Region(Guid.CreateVersion7(), "TR", "Türkiye", "TRY");
        var locale = new Locale(Guid.CreateVersion7(), "tr-TR", "tr", country, "Turkish", "Türkçe", true);
        var mapping = new LocaleCountry(locale, country, true);

        mapping.SetEnabled(false);

        Assert.True(mapping.IsRequired);
        Assert.False(mapping.IsEnabled);
    }
}
