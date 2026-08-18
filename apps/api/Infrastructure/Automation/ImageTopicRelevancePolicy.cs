using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public static partial class ImageTopicRelevancePolicy
{
    private static readonly HashSet<string> GenericTerms = new(StringComparer.Ordinal)
    {
        "image", "visual", "photo", "photograph", "picture", "cover", "background", "scene",
        "gorsel", "fotograf", "resim", "kapak", "arka", "plan", "sahne", "yazisiz",
        "original", "unique", "creative", "modern", "abstract", "decorative", "stock",
        "ozgun", "yaratici", "soyut", "dekoratif", "modern"
    };

    private static readonly HashSet<string> ProhibitedTextTerms = new(StringComparer.Ordinal)
    {
        "text", "caption", "headline", "title", "logo", "watermark", "typography",
        "yazi", "baslik", "altyazi", "filigran", "tipografi"
    };

    public static bool IsRelevantSet(string title, string summary, string category, string? coverQuery,
        IReadOnlyList<string>? inlineQueries, IReadOnlyList<string>? altTexts)
        => Explain(title, summary, category, coverQuery, inlineQueries, altTexts).Length == 0;

    public static string[] Explain(string title, string summary, string category, string? coverQuery,
        IReadOnlyList<string>? inlineQueries, IReadOnlyList<string>? altTexts)
    {
        if (inlineQueries is not { Count: 2 } || altTexts is not { Count: 2 }) return ["Görsel sorgusu veya alt metin sayısı iki değil."];
        var topic = Tokens($"{title} {summary} {category}");
        if (topic.Count < 2) return ["Makale konusu yeterli ayırt edici sözcük taşımıyor."];
        var queries = new[] { coverQuery, inlineQueries[0], inlineQueries[1] };
        var reasons = new List<string>();
        for (var index = 0; index < queries.Length; index++)
            if (!IsConcreteAndRelevant(queries[index], topic)) reasons.Add($"Görsel sorgusu {index + 1} somut veya konuyla ilgili değil.");
        for (var index = 0; index < altTexts.Count; index++)
            if (!IsConcreteAndRelevant(altTexts[index], topic)) reasons.Add($"Alt metin {index + 1} somut veya konuyla ilgili değil.");
        var sets = queries.Select(query => Tokens(query!)).ToArray();
        if (TooSimilar(sets[0], sets[1]) || TooSimilar(sets[0], sets[2]) || TooSimilar(sets[1], sets[2])) reasons.Add("Görsel sahneleri birbirine fazla benziyor.");
        return reasons.ToArray();
    }

    private static bool IsConcreteAndRelevant(string? value, HashSet<string> topic)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is < 8 or > 180) return false;
        var normalized = Normalize(value);
        var tokens = Tokens(normalized);
        if (tokens.Where(ProhibitedTextTerms.Contains).Any(term =>
                !Regex.IsMatch(normalized, $@"\b(?:no|without)\s+(?:\w+\s+){{0,2}}{Regex.Escape(term)}\b", RegexOptions.CultureInvariant))) return false;
        var concrete = tokens.Except(GenericTerms).ToHashSet(StringComparer.Ordinal);
        return concrete.Count >= 2 && concrete.Any(candidate => topic.Any(topicToken =>
            candidate.Equals(topicToken, StringComparison.Ordinal) ||
            candidate.Length >= 5 && topicToken.Length >= 5 &&
            (candidate.StartsWith(topicToken, StringComparison.Ordinal) || topicToken.StartsWith(candidate, StringComparison.Ordinal))));
    }

    private static bool TooSimilar(HashSet<string> left, HashSet<string> right)
    {
        var a = left.Except(GenericTerms).ToHashSet(StringComparer.Ordinal);
        var b = right.Except(GenericTerms).ToHashSet(StringComparer.Ordinal);
        var union = a.Union(b).Count();
        return union > 0 && (double)a.Intersect(b).Count() / union >= 0.8;
    }

    private static HashSet<string> Tokens(string value)
    {
        return TokenSeparator().Split(Normalize(value)).Where(token => token.Length > 2).ToHashSet(StringComparer.Ordinal);
    }

    private static string Normalize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark) builder.Append(character);
        return builder.ToString().ToLowerInvariant();
    }

    // Keep Unicode letters. ASCII-only tokenization discarded Turkish dotless ı
    // and other locale-specific letters, producing false relevance failures for
    // otherwise concrete recipe scenes.
    [GeneratedRegex(@"[^\p{L}\p{Nd}]+", RegexOptions.CultureInvariant)]
    private static partial Regex TokenSeparator();
}
