using System.Net;
using System.Text.RegularExpressions;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public sealed record VisualBrief(string SectionContext, string Purpose, string VisualType, string TypeReason, string Prompt, string NegativePrompt);
public sealed record SectionVisualBrief(string Heading, int HeadingLevel, string Purpose, string VisualType, string TypeReason, string Prompt, string NegativePrompt);

public static partial class VisualBriefBuilder
{
    public static VisualBrief Build(string title, string summary, string body, string locale, string[] categoryNames)
    {
        var headings = Heading().Matches(body).Select(match => Clean(match.Groups[1].Value)).Where(x => x.Length > 0).Take(3).ToArray();
        var section = headings.FirstOrDefault() ?? summary.Trim();
        if (section.Length > 500) section = section[..500];
        var region = locale switch { "tr-TR" => "contemporary Turkey, culturally accurate Turkish context", "de-DE" => "contemporary Germany, culturally accurate German context", "fr-FR" => "contemporary France, culturally accurate French context", _ => "contemporary United States, culturally accurate context" };
        var categories = categoryNames.Length == 0 ? "editorial feature" : string.Join(", ", categoryNames.Take(3));
        var (visualType, typeReason, style) = SelectVisualType(title, summary, section, body);
        var purpose = body.Length > 1800 ? "Hero + section-led editorial visual" : "Hero editorial visual";
        var prompt = $"Create a {visualType} for ‘{title.Trim()}’. Story summary: {summary.Trim()}. Key section context: {section}. Topic desk: {categories}. Locale and geography: {region}. Visual approach: {style}. Show concrete subject matter directly tied to the story; one unmistakable focal point; editorially credible details; mobile-safe center composition with safe crop at 16:9; professional publication quality; entirely text-free.";
        const string negative = "text, letters, numbers, captions, logo, watermark, brand mark, signage, fake user interface, distorted hands, extra fingers, duplicate objects, incorrect perspective, impossible reflections, inaccurate technical parts, blurry subject, clickbait composition";
        return new(section, purpose, visualType, typeReason, prompt, negative);
    }

    public static SectionVisualBrief[] BuildSectionPlan(string title, string summary, string body, string locale, string[] categoryNames)
    {
        var matches = Section().Matches(body);
        var candidates = matches.Select((match, index) =>
        {
            var heading = Clean(match.Groups[2].Value);
            var end = index + 1 < matches.Count ? matches[index + 1].Index : body.Length;
            var text = Clean(body[(match.Index + match.Length)..end]);
            return new { Heading = heading, Level = int.Parse(match.Groups[1].Value), Text = text.Length > 700 ? text[..700] : text };
        }).Where(section => section.Heading.Length > 0 && section.Text.Length >= 80).ToArray();
        if (candidates.Length == 0) return [];

        var selected = candidates.Length <= 3 ? candidates : [candidates[0], candidates[candidates.Length / 2], candidates[^1]];
        return selected.Select(section =>
        {
            var brief = Build(title, summary, $"<h{section.Level}>{section.Heading}</h{section.Level}><p>{section.Text}</p>", locale, categoryNames);
            return new SectionVisualBrief(section.Heading, section.Level, "Section visual", brief.VisualType, brief.TypeReason, brief.Prompt, brief.NegativePrompt);
        }).ToArray();
    }

    private static (string Type, string Reason, string Style) SelectVisualType(string title, string summary, string section, string body)
    {
        var context = $"{title} {summary} {section} {body}".ToLowerInvariant();
        if (ContainsAny(context, "adım", "nasil", "nasıl", "kurulum", "yapilandir", "yapılandır", "workflow", "step-by-step"))
            return ("step-by-step editorial illustration", "The article explains a repeatable procedure.", "A precise sequential scene without labels, arrows, UI text, or numbered steps; visually distinct stages through composition alone");
        if (ContainsAny(context, "karşılaştır", "karsilastir", "versus", "farkları", "farklari", "comparison"))
            return ("comparison editorial illustration", "The article contrasts alternatives.", "A balanced side-by-side composition using physical differences only; no labels, badges, score cards, or product-logo imitation");
        if (ContainsAny(context, "veri", "oran", "istatistik", "trend", "araştırma", "arastirma", "survey"))
            return ("data-led editorial illustration", "The story is driven by measured evidence.", "A concrete data-informed scene using scale, quantity, and spatial relationships without charts containing text or invented values");
        if (ContainsAny(context, "güvenlik", "guvenlik", "deprem", "sağlık", "saglik", "elektrik", "batarya", "motor", "devre"))
            return ("technical editorial illustration", "Technical or physical accuracy is central to reader trust.", "A restrained technically plausible cutaway-style scene; no invented interface, labels, impossible components, or unsafe procedure");
        return ("natural editorial photograph", "A real-world scene communicates the topic most directly.", "Natural documentary photography, plausible environment, honest lighting, realistic materials and human anatomy; not a generic stock pose");
    }

    private static bool ContainsAny(string value, params string[] terms) => terms.Any(value.Contains);

    private static string Clean(string value) => WebUtility.HtmlDecode(Tags().Replace(value, " ")).Trim();
    [GeneratedRegex("<h[23][^>]*>(.*?)</h[23]>", RegexOptions.IgnoreCase | RegexOptions.Singleline)] private static partial Regex Heading();
    [GeneratedRegex("<h([23])[^>]*>(.*?)</h\\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline)] private static partial Regex Section();
    [GeneratedRegex("<[^>]+>")] private static partial Regex Tags();
}
