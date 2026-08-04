namespace Peletnapechkai.Api.Localization;

public static class SupportedLocales
{
    public const string Default = "tr-TR";

    public static readonly IReadOnlyList<string> All =
    [
        Default,
        "en-US",
        "de-DE"
    ];

    public static bool Contains(string locale) =>
        All.Contains(locale, StringComparer.OrdinalIgnoreCase);
}
