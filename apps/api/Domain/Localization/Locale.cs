using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Domain.Localization;

public sealed class Locale
{
    private Locale()
    {
    }

    public Locale(
        Guid id,
        string code,
        string languageCode,
        Region region,
        string displayName,
        string nativeName,
        bool isDefault)
    {
        ArgumentNullException.ThrowIfNull(region);

        Id = id;
        Code = code;
        LanguageCode = languageCode;
        Region = region;
        RegionId = region.Id;
        DisplayName = displayName;
        NativeName = nativeName;
        IsDefault = isDefault;
        IsEnabled = true;
    }

    internal Locale(
        Guid id,
        string code,
        string languageCode,
        Guid regionId,
        string displayName,
        string nativeName,
        bool isDefault)
    {
        Id = id;
        Code = code;
        LanguageCode = languageCode;
        RegionId = regionId;
        DisplayName = displayName;
        NativeName = nativeName;
        IsDefault = isDefault;
        IsEnabled = true;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string LanguageCode { get; private set; } = string.Empty;

    public Guid RegionId { get; private set; }

    public Region Region { get; private set; } = null!;

    public string DisplayName { get; private set; } = string.Empty;

    public string NativeName { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }

    public bool IsEnabled { get; private set; }

    public ICollection<ArticleLocalization> ArticleLocalizations { get; } = [];
}
