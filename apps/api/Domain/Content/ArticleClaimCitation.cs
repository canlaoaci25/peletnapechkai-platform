namespace Peletnapechkai.Api.Domain.Content;

public sealed class ArticleClaimCitation
{
    private ArticleClaimCitation() { }

    public ArticleClaimCitation(ArticleLocalization article, Source source, string claim, string? locator, Guid approvedByUserId, DateTimeOffset approvedAt)
    {
        ArgumentNullException.ThrowIfNull(article);
        ArgumentNullException.ThrowIfNull(source);
        if (approvedByUserId == Guid.Empty) throw new ArgumentException("An approving editor is required.", nameof(approvedByUserId));
        ArgumentException.ThrowIfNullOrWhiteSpace(claim);
        var normalizedClaim = claim.Trim();
        if (normalizedClaim.Length > 500) throw new ArgumentOutOfRangeException(nameof(claim));
        var normalizedLocator = string.IsNullOrWhiteSpace(locator) ? null : locator.Trim();
        if (normalizedLocator?.Length > 240) throw new ArgumentOutOfRangeException(nameof(locator));

        Id = Guid.CreateVersion7();
        ArticleLocalization = article;
        ArticleLocalizationId = article.Id;
        Source = source;
        SourceId = source.Id;
        Claim = normalizedClaim;
        Locator = normalizedLocator;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = approvedAt;
        article.ClaimCitations.Add(this);
    }

    public Guid Id { get; private set; }
    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization ArticleLocalization { get; private set; } = null!;
    public Guid SourceId { get; private set; }
    public Source Source { get; private set; } = null!;
    public string Claim { get; private set; } = string.Empty;
    public string? Locator { get; private set; }
    public Guid ApprovedByUserId { get; private set; }
    public DateTimeOffset ApprovedAt { get; private set; }
}
