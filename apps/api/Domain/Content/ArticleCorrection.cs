namespace Peletnapechkai.Api.Domain.Content;

public sealed class ArticleCorrection
{
    private ArticleCorrection() { }

    public ArticleCorrection(ArticleLocalization article, string summary, string details, Guid approvedByUserId, DateTimeOffset correctedAt)
    {
        ArgumentNullException.ThrowIfNull(article);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(details);
        if (approvedByUserId == Guid.Empty) throw new ArgumentException("An approving editor is required.", nameof(approvedByUserId));
        if (summary.Trim().Length > 240) throw new ArgumentOutOfRangeException(nameof(summary));
        if (details.Trim().Length > 2000) throw new ArgumentOutOfRangeException(nameof(details));
        Id = Guid.CreateVersion7(); Article = article; ArticleLocalizationId = article.Id;
        Summary = summary.Trim(); Details = details.Trim(); ApprovedByUserId = approvedByUserId; CorrectedAt = correctedAt;
    }

    public Guid Id { get; private set; }
    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization Article { get; private set; } = null!;
    public string Summary { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public Guid ApprovedByUserId { get; private set; }
    public DateTimeOffset CorrectedAt { get; private set; }
}
