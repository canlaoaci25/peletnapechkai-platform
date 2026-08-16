using System.Text.RegularExpressions;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public sealed record ArticleVisualQualityInput(
    string Title, string Summary, string Body, string? AltText, string? Credit,
    int? Width, int? Height, long? OptimizedBytes, bool HasCover);

public sealed record ArticleVisualQualityResult(int Score, string Grade, string[] Risks, int BodyImageCount)
{
    public bool PassesPublicationGate => Score >= 80 && Risks.Length == 0;
}

public static partial class ArticleVisualQualityPolicy
{
    private static readonly string[] TextRiskTerms = ["logo", "watermark", "filigran", "başlık", "headline", "text", "yazı"];

    public static ArticleVisualQualityResult Assess(ArticleVisualQualityInput input)
    {
        var risks = new List<string>();
        var score = 100;
        var bodyImages = ImageTag().Matches(input.Body ?? string.Empty).Count;
        if (!input.HasCover) { risks.Add("missing-cover"); score -= 45; }
        if (input.HasCover && string.IsNullOrWhiteSpace(input.AltText)) { risks.Add("missing-alt"); score -= 25; }
        if (input.HasCover && !string.IsNullOrWhiteSpace(input.AltText) && !SharesTopic(input.Title + " " + input.Summary, input.AltText)) { risks.Add("topic-mismatch"); score -= 25; }
        if (input.HasCover && TextRiskTerms.Any(term => (input.AltText ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase))) { risks.Add("text-risk"); score -= 20; }
        if (input.HasCover && (input.Width is null || input.Height is null)) { risks.Add("unknown-dimensions"); score -= 10; }
        if (input.Width is > 0 && input.Height is > 0 && Math.Abs(input.Width.Value / (double)input.Height.Value - 16d / 9d) > .18) { risks.Add("unsafe-crop"); score -= 12; }
        if (input.HasCover && input.OptimizedBytes is null) { risks.Add("not-optimized"); score -= 12; }
        if (input.OptimizedBytes is > 450_000) { risks.Add("oversized"); score -= 10; }
        if (input.HasCover && string.IsNullOrWhiteSpace(input.Credit)) { risks.Add("missing-rights"); score -= 12; }
        if (bodyImages == 0 && PlainText().Replace(input.Body ?? string.Empty, " ").Length > 1400) { risks.Add("missing-body-visual"); score -= 8; }
        score = Math.Clamp(score, 0, 100);
        return new(score, score >= 90 ? "A" : score >= 80 ? "B" : score >= 65 ? "C" : "D", risks.Distinct().ToArray(), bodyImages);
    }

    private static bool SharesTopic(string topic, string alt)
    {
        var topicTerms = Words().Matches(topic.ToLowerInvariant()).Select(match => match.Value).Where(word => word.Length >= 5).ToHashSet();
        return Words().Matches(alt.ToLowerInvariant()).Select(match => match.Value).Any(topicTerms.Contains);
    }

    [GeneratedRegex("<img\\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)] private static partial Regex ImageTag();
    [GeneratedRegex("<[^>]+>", RegexOptions.CultureInvariant)] private static partial Regex PlainText();
    [GeneratedRegex("[\\p{L}\\p{N}]+", RegexOptions.CultureInvariant)] private static partial Regex Words();
}
