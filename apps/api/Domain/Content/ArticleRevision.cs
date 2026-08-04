namespace Peletnapechkai.Api.Domain.Content;

public sealed class ArticleRevision
{
    private ArticleRevision() { }

    public ArticleRevision(ArticleLocalization article, int number, string title, string summary, string body, Guid? createdByUserId, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(article);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Id = Guid.CreateVersion7();
        ArticleLocalization = article;
        ArticleLocalizationId = article.Id;
        Number = number;
        Title = title.Trim();
        Summary = summary.Trim();
        Body = body;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization ArticleLocalization { get; private set; } = null!;
    public int Number { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public Guid? CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
