using Ganss.Xss;

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

        return sanitizer.Sanitize(body ?? string.Empty);
    }
}
