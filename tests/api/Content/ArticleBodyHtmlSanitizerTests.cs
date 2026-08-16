using Peletnapechkai.Api.Infrastructure.Publishing;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class ArticleBodyHtmlSanitizerTests
{
    [Fact]
    public void Responsive_inline_image_hints_and_style_hook_are_preserved()
    {
        const string body = "<figure class=\"article-inline-image\"><img src=\"/api/media/123\" alt=\"Konu görseli\" width=\"1200\" height=\"675\" loading=\"lazy\" decoding=\"async\"></figure>";

        var sanitized = ArticleBodyHtmlSanitizer.Sanitize(body);

        Assert.Contains("class=\"article-inline-image\"", sanitized);
        Assert.Contains("loading=\"lazy\"", sanitized);
        Assert.Contains("decoding=\"async\"", sanitized);
        Assert.Contains("width=\"1200\"", sanitized);
        Assert.Contains("height=\"675\"", sanitized);
    }

    [Fact]
    public void Inline_images_are_normalized_for_accessibility_and_deferred_rendering()
    {
        const string body = "<p>Metin</p><img src=\"/api/media/123\"><img src=\"/api/media/456\" alt=\"Konuya özel gövde görseli\" loading=\"eager\" decoding=\"sync\">";

        var sanitized = ArticleBodyHtmlSanitizer.Sanitize(body);

        Assert.Equal(2, CountOccurrences(sanitized, "loading=\"lazy\""));
        Assert.Equal(2, CountOccurrences(sanitized, "decoding=\"async\""));
        Assert.Contains("alt=\"\"", sanitized);
        Assert.Contains("alt=\"Konuya özel gövde görseli\"", sanitized);
        Assert.DoesNotContain("loading=\"eager\"", sanitized);
        Assert.DoesNotContain("decoding=\"sync\"", sanitized);
    }

    [Fact]
    public void Executable_markup_and_event_handlers_are_removed()
    {
        const string body = "<script>alert(1)</script><img src=\"javascript:alert(2)\" onerror=\"alert(3)\" alt=\"Güvenli açıklama\">";

        var sanitized = ArticleBodyHtmlSanitizer.Sanitize(body);

        Assert.DoesNotContain("<script", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alt=\"Güvenli açıklama\"", sanitized);
    }

    private static int CountOccurrences(string value, string search) =>
        value.Split(search, StringSplitOptions.None).Length - 1;
}
