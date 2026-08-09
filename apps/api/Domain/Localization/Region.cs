namespace Peletnapechkai.Api.Domain.Localization;

public sealed class Region
{
    private Region()
    {
    }

    public Region(Guid id, string code, string name, string currencyCode)
    {
        Id = id;
        Code = code;
        Name = name;
        CurrencyCode = currencyCode;
        IsEnabled = true;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string CurrencyCode { get; private set; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public ICollection<Locale> Locales { get; } = [];
    public ICollection<LocaleCountry> LocaleCountries { get; } = [];
}
