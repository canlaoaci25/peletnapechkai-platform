namespace Peletnapechkai.Api.Domain.Automation;

public enum VisualReviewStatus { Pending, InReview, Approved, Rejected, RetryRequested }
public enum VisualReviewTarget { Cover, BodySection }

public sealed class VisualReviewTask
{
    private VisualReviewTask() { }

    public VisualReviewTask(Guid articleLocalizationId, Guid? currentMediaAssetId, int qualityScore,
        string risks, string sectionContext, string visualPurpose, string proposedPrompt,
        string negativePrompt, string idempotencyKey, DateTimeOffset now, Guid? automationJobId = null,
        VisualReviewTarget target = VisualReviewTarget.Cover, string? sectionHeading = null)
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
        if (target == VisualReviewTarget.BodySection && string.IsNullOrWhiteSpace(sectionHeading))
            throw new ArgumentException("A body visual requires its section heading.", nameof(sectionHeading));
        Target = target; SectionHeading = string.IsNullOrWhiteSpace(sectionHeading) ? null : sectionHeading.Trim();
        IdempotencyKey = idempotencyKey.Trim(); AutomationJobId = automationJobId; Status = VisualReviewStatus.Pending; CreatedAt = now; UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid ArticleLocalizationId { get; private set; }
    public Guid? AutomationJobId { get; private set; }
    public Guid? CurrentMediaAssetId { get; private set; }
    public Guid? CandidateMediaAssetId { get; private set; }
    public int QualityScore { get; private set; }
    public string Risks { get; private set; } = "";
    public string SectionContext { get; private set; } = "";
    public string VisualPurpose { get; private set; } = "";
    public string ProposedPrompt { get; private set; } = "";
    public string NegativePrompt { get; private set; } = "";
    public VisualReviewTarget Target { get; private set; }
    public string? SectionHeading { get; private set; }
    public string IdempotencyKey { get; private set; } = "";
    public VisualReviewStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public string? ReviewerNote { get; private set; }
    public string? Provider { get; private set; }
    public string? LicenseName { get; private set; }
    public string? Attribution { get; private set; }
    public string? CandidateAltText { get; private set; }
    public int? TopicScore { get; private set; }
    public int? TextSafetyScore { get; private set; }
    public int? CropScore { get; private set; }
    public int? OriginalityScore { get; private set; }
    public Guid? ClosestMediaAssetId { get; private set; }
    public int? ClosestSimilarityPercent { get; private set; }
    public DateTimeOffset? PromotedAt { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void ChangeStatus(VisualReviewStatus status, Guid actorUserId, string? note, DateTimeOffset now)
    {
        if (actorUserId == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(actorUserId));
        if (status == VisualReviewStatus.RetryRequested) AttemptCount++;
        if (status is VisualReviewStatus.RetryRequested or VisualReviewStatus.Rejected) InvalidateCandidateEvidence();
        Status = status; ReviewerNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        ReviewedByUserId = actorUserId; ReviewedAt = now; UpdatedAt = now;
    }

    public void AttachCandidate(Guid mediaAssetId, string provider, string licenseName, string? attribution,
        string altText, bool articleConfirmed, bool sectionConfirmed, bool localeConfirmed, bool technicalAccuracyConfirmed,
        bool textAndLogoFreeConfirmed, bool artifactFreeConfirmed, bool cropConfirmed, Guid actorUserId, int cropScore,
        int originalityScore, Guid? closestMediaAssetId, int closestSimilarityPercent, DateTimeOffset now)
    {
        if (mediaAssetId == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(mediaAssetId));
        ArgumentException.ThrowIfNullOrWhiteSpace(provider); ArgumentException.ThrowIfNullOrWhiteSpace(licenseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(altText);
        if (actorUserId == Guid.Empty) throw new ArgumentOutOfRangeException(nameof(actorUserId));
        CandidateMediaAssetId = mediaAssetId; Provider = provider.Trim(); LicenseName = licenseName.Trim();
        Attribution = string.IsNullOrWhiteSpace(attribution) ? null : attribution.Trim(); CandidateAltText = altText.Trim();
        TopicScore = articleConfirmed && sectionConfirmed && localeConfirmed && technicalAccuracyConfirmed ? 100 : 0;
        TextSafetyScore = textAndLogoFreeConfirmed && artifactFreeConfirmed ? 100 : 0;
        CropScore = cropConfirmed ? ClampScore(cropScore) : 0;
        OriginalityScore = ClampScore(originalityScore); Status = VisualReviewStatus.InReview; UpdatedAt = now;
        ClosestMediaAssetId = closestMediaAssetId; ClosestSimilarityPercent = ClampScore(closestSimilarityPercent);
        ReviewedByUserId = actorUserId; ReviewedAt = now;
    }

    public bool CandidatePasses => Status == VisualReviewStatus.InReview && CandidateMediaAssetId.HasValue && TopicScore >= 80 && TextSafetyScore >= 95 &&
        CropScore >= 80 && OriginalityScore >= 85 && ReviewedByUserId.HasValue && ReviewedAt.HasValue &&
        !string.IsNullOrWhiteSpace(LicenseName) && !string.IsNullOrWhiteSpace(CandidateAltText);

    public void MarkPromoted(Guid actorUserId, string note, DateTimeOffset now)
    {
        if (!CandidatePasses) throw new InvalidOperationException("Candidate has not passed every publication gate.");
        ChangeStatus(VisualReviewStatus.Approved, actorUserId, note, now); PromotedAt = now;
    }

    private static int ClampScore(int value) => Math.Clamp(value, 0, 100);

    private void InvalidateCandidateEvidence()
    {
        CandidateMediaAssetId = null; Provider = null; LicenseName = null; Attribution = null; CandidateAltText = null;
        TopicScore = null; TextSafetyScore = null; CropScore = null; OriginalityScore = null;
        ClosestMediaAssetId = null; ClosestSimilarityPercent = null; ReviewedByUserId = null; ReviewedAt = null;
    }
}
