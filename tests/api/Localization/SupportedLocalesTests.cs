using Peletnapechkai.Api.Localization;

namespace Peletnapechkai.Api.Tests.Localization;

public sealed class SupportedLocalesTests
{
    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("TR-tr")]
    public void Contains_ReturnsTrue_ForSupportedLocales(string locale)
    {
        Assert.True(SupportedLocales.Contains(locale));
    }

    [Theory]
    [InlineData("tr")]
    [InlineData("fr-FR")]
    [InlineData("")]
    public void Contains_ReturnsFalse_ForUnsupportedLocales(string locale)
    {
        Assert.False(SupportedLocales.Contains(locale));
    }
}
