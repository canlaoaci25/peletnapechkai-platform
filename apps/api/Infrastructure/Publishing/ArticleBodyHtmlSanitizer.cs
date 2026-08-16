using Ganss.Xss;
using AngleSharp.Dom;

namespace Peletnapechkai.Api.Infrastructure.Publishing;

public static class ArticleBodyHtmlSanitizer
{
    public static string Sanitize(string? body)
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[] { "p", "br", "h2", "h3", "h4", "strong", "em", "u", "s", "blockquote", "ul", "ol", "li", "pre", "code", "a", "img", "figure", "figcaption", "hr", "table", "thead", "tbody", "tfoot", "tr", "th", "td", "video", "audio", "source" })
            sanitizer.AllowedTags.Add(tag);

        sanitizer.AllowedAttributes.Clear();
        foreach (var attribute in new[] { "href", "target", "rel", "src", "alt", "title", "width", "height", "colspan", "rowspan", "controls", "poster", "preload", "class", "loading", "decoding" })
            sanitizer.AllowedAttributes.Add(attribute);

        sanitizer.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "http", "https" })
            sanitizer.AllowedSchemes.Add(scheme);

        sanitizer.PostProcessNode += (_, eventArgs) =>
        {
            if (eventArgs.Node is not IElement { LocalName: "img" } image)
                return;

            // Article-body images follow the primary cover and need not block rendering.
            image.SetAttribute("loading", "lazy");
            image.SetAttribute("decoding", "async");

            // Without alt, browsers may announce the source filename. Preserve any
            // editorial text, but use an empty alternative when the attribute is absent.
            if (!image.HasAttribute("alt"))
                image.SetAttribute("alt", string.Empty);
        };

        return sanitizer.Sanitize(body ?? string.Empty);
    }
}
