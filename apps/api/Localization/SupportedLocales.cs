using System.Text.Json;

namespace Peletnapechkai.Api.Localization;

public static class SupportedLocales
{
    private sealed record LocaleConfiguration(string DefaultLocale, Dictionary<string, string> Locales);

    private static readonly LocaleConfiguration Configuration = Load();

    public static string Default => Configuration.DefaultLocale;

    public static IReadOnlyList<string> All { get; } = Configuration.Locales.Keys.ToArray();

    public static bool Contains(string locale) =>
        All.Contains(locale, StringComparer.OrdinalIgnoreCase);

    private static LocaleConfiguration Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "supported-locales.json");
        if (!File.Exists(path)) throw new InvalidOperationException($"Locale configuration is missing: {path}");
        var configuration = JsonSerializer.Deserialize<LocaleConfiguration>(File.ReadAllText(path), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (configuration is null || configuration.Locales.Count == 0 || !configuration.Locales.ContainsKey(configuration.DefaultLocale))
            throw new InvalidOperationException("Locale configuration is invalid or its default locale is missing.");
        return configuration;
    }
}
