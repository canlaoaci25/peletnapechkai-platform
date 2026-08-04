using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Domain.Content;

public sealed class ArticleLocalization
{
    private ArticleLocalization()
    {
    }

    public ArticleLocalization(
        ArticleGroup articleGroup,
        Locale locale,
        string slug,
        string title,
        string summary,
        string body,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(articleGroup);
        ArgumentNullException.ThrowIfNull(locale);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Id = Guid.CreateVersion7();
        ArticleGroup = articleGroup;
        ArticleGroupId = articleGroup.Id;
        Locale = locale;
        LocaleId = locale.Id;
        Slug = slug.Trim();
        Title = title.Trim();
        Summary = summary.Trim();
        Body = body;
        Status = PublicationStatus.Draft;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Guid ArticleGroupId { get; private set; }

    public ArticleGroup ArticleGroup { get; private set; } = null!;

    public Guid LocaleId { get; private set; }

    public Locale Locale { get; private set; } = null!;

    public string Slug { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public string? SeoTitle { get; private set; }

    public string? SeoDescription { get; private set; }

    public PublicationStatus Status { get; private set; }

    public DateTimeOffset? ScheduledAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public SeoMetadata? SeoMetadata { get; private set; }

    public ICollection<ArticleRevision> Revisions { get; } = [];

    public ICollection<Category> Categories { get; } = [];

    public ICollection<Tag> Tags { get; } = [];
}
