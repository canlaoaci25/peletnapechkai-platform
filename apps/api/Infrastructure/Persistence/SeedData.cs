using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Infrastructure.Persistence;

public static class SeedData
{
    public static readonly Guid TurkeyRegionId = Guid.Parse("0198F100-0000-7000-8000-000000000001");
    public static readonly Guid UnitedStatesRegionId = Guid.Parse("0198F100-0000-7000-8000-000000000002");
    public static readonly Guid GermanyRegionId = Guid.Parse("0198F100-0000-7000-8000-000000000003");
    public static readonly Guid FranceRegionId = Guid.Parse("0198F100-0000-7000-8000-000000000004");

    public static readonly Guid TurkishLocaleId = Guid.Parse("0198F100-0000-7000-9000-000000000001");
    public static readonly Guid AmericanEnglishLocaleId = Guid.Parse("0198F100-0000-7000-9000-000000000002");
    public static readonly Guid GermanLocaleId = Guid.Parse("0198F100-0000-7000-9000-000000000003");
    public static readonly Guid FrenchLocaleId = Guid.Parse("0198F100-0000-7000-9000-000000000004");

    public static readonly Region[] Regions =
    [
        new(TurkeyRegionId, "TR", "Türkiye", "TRY"),
        new(UnitedStatesRegionId, "US", "United States", "USD"),
        new(GermanyRegionId, "DE", "Germany", "EUR"),
        new(FranceRegionId, "FR", "France", "EUR")
    ];

    public static readonly Locale[] Locales =
    [
        new(TurkishLocaleId, "tr-TR", "tr", TurkeyRegionId, "Turkish (Türkiye)", "Türkçe (Türkiye)", true),
        new(AmericanEnglishLocaleId, "en-US", "en", UnitedStatesRegionId, "English (United States)", "English (United States)", false),
        new(GermanLocaleId, "de-DE", "de", GermanyRegionId, "German (Germany)", "Deutsch (Deutschland)", false),
        new(FrenchLocaleId, "fr-FR", "fr", FranceRegionId, "French (France)", "Français (France)", false)
    ];
}
