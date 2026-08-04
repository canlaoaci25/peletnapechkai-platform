namespace Peletnapechkai.Api.Domain.Content;

public sealed class SeoMetadata
{
    private SeoMetadata() { }

    public SeoMetadata(ArticleLocalization article)
    {
        ArgumentNullException.ThrowIfNull(article);
        ArticleLocalizationId = article.Id;
        ArticleLocalization = article;
    }

    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization ArticleLocalization { get; private set; } = null!;
    public string? CanonicalUrl { get; private set; }
    public string? OpenGraphTitle { get; private set; }
    public string? OpenGraphDescription { get; private set; }
    public string? RobotsDirective { get; private set; }
    public string? StructuredDataJson { get; private set; }
}
