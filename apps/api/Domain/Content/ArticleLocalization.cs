using Peletnapechkai.Api.Domain.Localization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

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
        articleGroup.Localizations.Add(this);
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
    public Guid? CoverMediaAssetId { get; private set; }
    public MediaAsset? CoverMediaAsset { get; private set; }
    public string? CoverAltText { get; private set; }
    public string? CoverCaption { get; private set; }
    public string? CoverCredit { get; private set; }
    public bool IsSponsored { get; private set; }
    public string? SponsorName { get; private set; }
    public bool HasAffiliateLinks { get; private set; }

    public PublicationStatus Status { get; private set; }

    public DateTimeOffset? ScheduledAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }
    public Guid? GeneratedByAutomationJobId { get; private set; }
    public DateTimeOffset? SourceSnapshotUpdatedAt { get; private set; }
    public string? SourceTitleHash { get; private set; }
    public string? SourceSummaryHash { get; private set; }
    public string? SourceBodyHash { get; private set; }
    public string? SourceSeoHash { get; private set; }

    public SeoMetadata? SeoMetadata { get; private set; }

    public ICollection<ArticleRevision> Revisions { get; } = [];
    public ICollection<ArticleCorrection> Corrections { get; } = [];
    public ICollection<ArticleClaimCitation> ClaimCitations { get; } = [];

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

    public void UpdateCommercialDisclosure(bool isSponsored, string? sponsorName, bool hasAffiliateLinks, DateTimeOffset updatedAt)
    {
        if (Status != PublicationStatus.Draft) throw new InvalidOperationException("Only draft disclosures can be edited.");
        if (isSponsored && string.IsNullOrWhiteSpace(sponsorName)) throw new ArgumentException("Sponsored content requires a sponsor name.", nameof(sponsorName));
        IsSponsored=isSponsored; SponsorName=isSponsored?sponsorName!.Trim():null; HasAffiliateLinks=hasAffiliateLinks; UpdatedAt=updatedAt;
    }

    public void UpdateCover(MediaAsset? asset, string? altText, string? caption, string? credit, DateTimeOffset updatedAt)
    {
        if (Status != PublicationStatus.Draft) throw new InvalidOperationException("Only draft covers can be edited.");
        if (asset is not null && string.IsNullOrWhiteSpace(altText)) throw new ArgumentException("Cover images require alternative text.", nameof(altText));
        CoverMediaAsset = asset;
        CoverMediaAssetId = asset?.Id;
        CoverAltText = asset is null ? null : altText!.Trim();
        CoverCaption = asset is null || string.IsNullOrWhiteSpace(caption) ? null : caption.Trim();
        CoverCredit = asset is null || string.IsNullOrWhiteSpace(credit) ? null : credit.Trim();
        UpdatedAt = updatedAt;
    }

    public void PromoteReviewedCover(MediaAsset asset, string altText, string credit, DateTimeOffset updatedAt)
    {
        if (Status != PublicationStatus.Published) throw new InvalidOperationException("Only published articles can receive reviewed covers.");
        ArgumentNullException.ThrowIfNull(asset); ArgumentException.ThrowIfNullOrWhiteSpace(altText); ArgumentException.ThrowIfNullOrWhiteSpace(credit);
        CoverMediaAsset = asset; CoverMediaAssetId = asset.Id; CoverAltText = altText.Trim(); CoverCaption = null;
        CoverCredit = credit.Trim(); UpdatedAt = updatedAt;
    }

    public void PromoteReviewedBodyImage(MediaAsset asset, string sectionHeading, string altText, string credit, DateTimeOffset updatedAt)
    {
        if (Status != PublicationStatus.Published) throw new InvalidOperationException("Only published articles can receive reviewed body images.");
        ArgumentNullException.ThrowIfNull(asset); ArgumentException.ThrowIfNullOrWhiteSpace(sectionHeading);
        ArgumentException.ThrowIfNullOrWhiteSpace(altText); ArgumentException.ThrowIfNullOrWhiteSpace(credit);
        if (asset.Width is null || asset.Height is null) throw new InvalidOperationException("Reviewed body images require intrinsic dimensions.");
        var headingPattern = $"(<h(?<level>[23])[^>]*>\\s*{Regex.Escape(sectionHeading.Trim())}\\s*</h\\k<level>>)";
        var match = Regex.Match(Body, headingPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success) throw new InvalidOperationException("The reviewed section heading no longer exists; refresh the brief before promotion.");
        var marker = $"data-visual-section=\"{WebUtility.HtmlEncode(sectionHeading.Trim())}\"";
        if (Body.Contains(marker, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("This section already has a promoted visual.");
        var figure = $"<figure class=\"article-inline-image\" {marker}><img src=\"/api/media/{asset.Id}?v={asset.OptimizedByteLength}\" alt=\"{WebUtility.HtmlEncode(altText.Trim())}\" width=\"{asset.Width}\" height=\"{asset.Height}\" loading=\"lazy\" decoding=\"async\"><figcaption>{WebUtility.HtmlEncode(credit.Trim())}</figcaption></figure>";
        Body = Body.Insert(match.Index + match.Length, figure);
        UpdatedAt = updatedAt;
    }

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

    public void PublishAutomatedTranslation(DateTimeOffset publishedAt)
    {
        if (Locale.IsDefault || Status != PublicationStatus.Draft)
        {
            throw new InvalidOperationException("Only non-default draft translations can be automatically published.");
        }

        Status = PublicationStatus.Published;
        PublishedAt = publishedAt;
        ScheduledAt = null;
        UpdatedAt = publishedAt;
    }

    public void PublishAutomatedSource(Guid jobId, DateTimeOffset publishedAt)
    {
        if (!Locale.IsDefault || Status != PublicationStatus.Draft || jobId == Guid.Empty)
        {
            throw new InvalidOperationException("Only default-locale draft automation sources can be automatically published.");
        }
        GeneratedByAutomationJobId = jobId;
        Status = PublicationStatus.Published;
        PublishedAt = publishedAt;
        ScheduledAt = null;
        UpdatedAt = publishedAt;
    }

    public void MarkGeneratedTranslation(Guid jobId)
    {
        if (Locale.IsDefault || jobId == Guid.Empty) throw new InvalidOperationException("Only translated content can be linked to a generation job.");
        GeneratedByAutomationJobId = jobId;
    }

    public void CaptureSourceSnapshot(ArticleLocalization source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (Locale.IsDefault || !source.Locale.IsDefault || source.ArticleGroupId != ArticleGroupId)
            throw new InvalidOperationException("A translation snapshot requires its default-locale source.");
        SourceSnapshotUpdatedAt = source.UpdatedAt;
        SourceTitleHash = Hash(source.Title);
        SourceSummaryHash = Hash(source.Summary);
        SourceBodyHash = Hash(source.Body);
        SourceSeoHash = Hash($"{source.SeoTitle}\n{source.SeoDescription}");
    }

    public IReadOnlyList<string> ChangedSourceFields(ArticleLocalization source)
    {
        if (SourceSnapshotUpdatedAt is null) return ["Untracked"];
        var fields = new List<string>(4);
        if (SourceTitleHash != Hash(source.Title)) fields.Add("Title");
        if (SourceSummaryHash != Hash(source.Summary)) fields.Add("Summary");
        if (SourceBodyHash != Hash(source.Body)) fields.Add("Body");
        if (SourceSeoHash != Hash($"{source.SeoTitle}\n{source.SeoDescription}")) fields.Add("Seo");
        return fields;
    }

    private static string Hash(string? value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)));

    public void UpdateAutomatedSeo(string seoTitle, string seoDescription, DateTimeOffset updatedAt)
    {
        if (Locale.IsDefault || Status is not (PublicationStatus.Draft or PublicationStatus.Published))
        {
            throw new InvalidOperationException("Only non-default draft or published translations can receive automated SEO.");
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(seoTitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(seoDescription);
        SeoTitle = seoTitle.Trim();
        SeoDescription = seoDescription.Trim();
        UpdatedAt = updatedAt;
    }

    public void UpdateGeneratedSeo(Guid jobId, string seoTitle, string seoDescription, DateTimeOffset updatedAt)
    {
        if (GeneratedByAutomationJobId != jobId || Status != PublicationStatus.Published)
            throw new InvalidOperationException("Only published content from the same generation job can receive generated SEO.");
        ArgumentException.ThrowIfNullOrWhiteSpace(seoTitle); ArgumentException.ThrowIfNullOrWhiteSpace(seoDescription);
        SeoTitle = seoTitle.Trim(); SeoDescription = seoDescription.Trim(); UpdatedAt = updatedAt;
    }

    public void RefreshGeneratedCover(Guid jobId, MediaAsset asset, string altText, string? caption, string credit, DateTimeOffset updatedAt)
    {
        if (GeneratedByAutomationJobId != jobId || Status != PublicationStatus.Published)
            throw new InvalidOperationException("Only published content from the same generation job can refresh its cover.");
        ArgumentException.ThrowIfNullOrWhiteSpace(altText); ArgumentException.ThrowIfNullOrWhiteSpace(credit);
        CoverMediaAsset = asset; CoverMediaAssetId = asset.Id; CoverAltText = altText.Trim();
        CoverCaption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(); CoverCredit = credit.Trim(); UpdatedAt = updatedAt;
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
