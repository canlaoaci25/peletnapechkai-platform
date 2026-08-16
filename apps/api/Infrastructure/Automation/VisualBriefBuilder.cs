using System.Net;
using System.Text.RegularExpressions;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public sealed record VisualBrief(string SectionContext, string Purpose, string Prompt, string NegativePrompt);

public static partial class VisualBriefBuilder
{
    public static VisualBrief Build(string title, string summary, string body, string locale, string[] categoryNames)
    {
        var headings = Heading().Matches(body).Select(match => Clean(match.Groups[1].Value)).Where(x => x.Length > 0).Take(3).ToArray();
        var section = headings.FirstOrDefault() ?? summary.Trim();
        if (section.Length > 500) section = section[..500];
        var region = locale switch { "tr-TR" => "contemporary Turkey, culturally accurate Turkish context", "de-DE" => "contemporary Germany, culturally accurate German context", "fr-FR" => "contemporary France, culturally accurate French context", _ => "contemporary United States, culturally accurate context" };
        var categories = categoryNames.Length == 0 ? "editorial feature" : string.Join(", ", categoryNames.Take(3));
        var purpose = body.Length > 1800 ? "Hero + section-led editorial visual" : "Hero editorial visual";
        var prompt = $"Original text-free editorial visual for ‘{title.Trim()}’. Story summary: {summary.Trim()}. Key section context: {section}. Topic desk: {categories}. {region}. Show concrete subject matter and a plausible real scene directly tied to the story; clear single focal point; mobile-safe center composition; natural editorial lighting; professional publication quality; no decorative abstraction; 16:9.";
        const string negative = "text, letters, numbers, captions, logo, watermark, brand mark, signage, fake user interface, distorted hands, extra fingers, duplicate objects, incorrect perspective, impossible reflections, inaccurate technical parts, blurry subject, clickbait composition";
        return new(section, purpose, prompt, negative);
    }

    private static string Clean(string value) => WebUtility.HtmlDecode(Tags().Replace(value, " ")).Trim();
    [GeneratedRegex("<h[23][^>]*>(.*?)</h[23]>", RegexOptions.IgnoreCase | RegexOptions.Singleline)] private static partial Regex Heading();
    [GeneratedRegex("<[^>]+>")] private static partial Regex Tags();
}
