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

    public void UpdateDraft(string slug, string title, string summary, string body, string? seoTitle, string? seoDescription, DateTimeOffset updatedAt)
    {
        if (Status != PublicationStatus.Draft)
        {
            throw new InvalidOperationException("Only draft articles can be edited.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Slug = slug.Trim();
        Title = title.Trim();
        Summary = summary.Trim();
        Body = body;
        SeoTitle = string.IsNullOrWhiteSpace(seoTitle) ? null : seoTitle.Trim();
        SeoDescription = string.IsNullOrWhiteSpace(seoDescription) ? null : seoDescription.Trim();
        UpdatedAt = updatedAt;
    }

    public void SubmitForEditorialReview(DateTimeOffset updatedAt) =>
        Transition(PublicationStatus.Draft, PublicationStatus.InEditorialReview, updatedAt);

    public void ApproveEditorialReview(DateTimeOffset updatedAt) =>
        Transition(PublicationStatus.InEditorialReview, PublicationStatus.InSeoReview, updatedAt);

    public void ReturnToDraft(DateTimeOffset updatedAt)
    {
        if (Status is not (PublicationStatus.InEditorialReview or PublicationStatus.InSeoReview))
        {
            throw new InvalidOperationException("Only articles in review can return to draft.");
        }

        Status = PublicationStatus.Draft;
        ScheduledAt = null;
        UpdatedAt = updatedAt;
    }

    public void Schedule(DateTimeOffset scheduledAt, DateTimeOffset updatedAt)
    {
        if (Status != PublicationStatus.InSeoReview || scheduledAt <= updatedAt)
        {
            throw new InvalidOperationException("SEO-approved articles require a future schedule time.");
        }

        Status = PublicationStatus.Scheduled;
        ScheduledAt = scheduledAt;
        UpdatedAt = updatedAt;
    }

    public void Publish(DateTimeOffset publishedAt)
    {
        if (Status is not (PublicationStatus.InSeoReview or PublicationStatus.Scheduled))
        {
            throw new InvalidOperationException("Only SEO-reviewed or scheduled articles can be published.");
        }

        Status = PublicationStatus.Published;
        PublishedAt = publishedAt;
        ScheduledAt = null;
        UpdatedAt = publishedAt;
    }

    public void Archive(DateTimeOffset updatedAt)
    {
        if (Status != PublicationStatus.Published)
        {
            throw new InvalidOperationException("Only published articles can be archived.");
        }

        Status = PublicationStatus.Archived;
        UpdatedAt = updatedAt;
    }

    private void Transition(PublicationStatus expected, PublicationStatus target, DateTimeOffset updatedAt)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException($"Article must be {expected} before moving to {target}.");
        }

        Status = target;
        UpdatedAt = updatedAt;
    }
}
