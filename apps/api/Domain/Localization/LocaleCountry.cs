namespace Peletnapechkai.Api.Domain.Localization;

public sealed class LocaleCountry
{
    private LocaleCountry() { }

    public LocaleCountry(Locale locale, Region country, bool isRequired, bool isEnabled = true)
    {
        ArgumentNullException.ThrowIfNull(locale);
        ArgumentNullException.ThrowIfNull(country);
        Locale = locale;
        LocaleId = locale.Id;
        Country = country;
        CountryId = country.Id;
        IsRequired = isRequired;
        IsEnabled = isEnabled;
    }

    public Guid LocaleId { get; private set; }
    public Locale Locale { get; private set; } = null!;
    public Guid CountryId { get; private set; }
    public Region Country { get; private set; } = null!;
    public bool IsRequired { get; private set; }
    public bool IsEnabled { get; private set; }

    public void SetEnabled(bool enabled) => IsEnabled = enabled;
}
