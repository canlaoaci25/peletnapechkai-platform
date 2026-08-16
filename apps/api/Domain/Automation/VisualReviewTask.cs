namespace Peletnapechkai.Api.Domain.Automation;

public enum VisualReviewStatus { Pending, InReview, Approved, Rejected, RetryRequested }

public sealed class VisualReviewTask
{
    private VisualReviewTask() { }

    public VisualReviewTask(Guid articleLocalizationId, Guid? currentMediaAssetId, int qualityScore,
        string risks, string sectionContext, string visualPurpose, string proposedPrompt,
        string negativePrompt, string idempotencyKey, DateTimeOffset now)
    {
        if (articleLocalizationId == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(articleLocalizationId));
        ArgumentException.ThrowIfNullOrWhiteSpace(risks);
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(visualPurpose);
        ArgumentException.ThrowIfNullOrWhiteSpace(proposedPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(negativePrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        Id = Guid.CreateVersion7(); ArticleLocalizationId = articleLocalizationId; CurrentMediaAssetId = currentMediaAssetId;
        QualityScore = Math.Clamp(qualityScore, 0, 100); Risks = risks; SectionContext = sectionContext.Trim();
        VisualPurpose = visualPurpose.Trim(); ProposedPrompt = proposedPrompt.Trim(); NegativePrompt = negativePrompt.Trim();
        IdempotencyKey = idempotencyKey.Trim(); Status = VisualReviewStatus.Pending; CreatedAt = now; UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid ArticleLocalizationId { get; private set; }
    public Guid? CurrentMediaAssetId { get; private set; }
    public int QualityScore { get; private set; }
    public string Risks { get; private set; } = "";
    public string SectionContext { get; private set; } = "";
    public string VisualPurpose { get; private set; } = "";
    public string ProposedPrompt { get; private set; } = "";
    public string NegativePrompt { get; private set; } = "";
    public string IdempotencyKey { get; private set; } = "";
    public VisualReviewStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public string? ReviewerNote { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void ChangeStatus(VisualReviewStatus status, Guid actorUserId, string? note, DateTimeOffset now)
    {
        if (actorUserId == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(actorUserId));
        if (status == VisualReviewStatus.RetryRequested) AttemptCount++;
        Status = status; ReviewerNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        ReviewedByUserId = actorUserId; ReviewedAt = now; UpdatedAt = now;
    }
}
