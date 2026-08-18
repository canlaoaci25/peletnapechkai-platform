namespace Peletnapechkai.Api.Domain.Automation;

public enum VisualReviewStatus { Pending, InReview, Approved, Rejected, RetryRequested, DeadLetter }
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
    public Guid? LeaseToken { get; private set; }
    public string? LeaseOwner { get; private set; }
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public string? LastFailureCode { get; private set; }
    public DateTimeOffset? DeadLetteredAt { get; private set; }
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

    public bool IsAvailableForGeneration(DateTimeOffset now) =>
        Status is VisualReviewStatus.Pending or VisualReviewStatus.RetryRequested &&
        DeadLetteredAt is null && (NextAttemptAt is null || NextAttemptAt <= now) &&
        (LeaseToken is null || LeaseExpiresAt <= now);

    public Guid ClaimGeneration(string owner, DateTimeOffset now, TimeSpan leaseDuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        if (leaseDuration < TimeSpan.FromSeconds(30) || leaseDuration > TimeSpan.FromMinutes(30))
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        if (!IsAvailableForGeneration(now)) throw new InvalidOperationException("Visual task is not available for generation.");
        LeaseToken = Guid.CreateVersion7(); LeaseOwner = owner.Trim(); LeaseExpiresAt = now.Add(leaseDuration);
        UpdatedAt = now; return LeaseToken.Value;
    }

    public void RenewGenerationLease(Guid token, DateTimeOffset now, TimeSpan leaseDuration)
    {
        EnsureActiveLease(token, now);
        if (leaseDuration < TimeSpan.FromSeconds(30) || leaseDuration > TimeSpan.FromMinutes(30))
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        LeaseExpiresAt = now.Add(leaseDuration); UpdatedAt = now;
    }

    public void RecordGenerationFailure(Guid token, string failureCode, DateTimeOffset now, int maxAttempts = 3)
    {
        EnsureActiveLease(token, now); ArgumentException.ThrowIfNullOrWhiteSpace(failureCode);
        if (maxAttempts is < 1 or > 10) throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        AttemptCount++; LastFailureCode = NormalizeFailureCode(failureCode); ClearLease();
        if (AttemptCount >= maxAttempts)
        {
            Status = VisualReviewStatus.DeadLetter; DeadLetteredAt = now; NextAttemptAt = null;
        }
        else
        {
            Status = VisualReviewStatus.RetryRequested;
            NextAttemptAt = now.AddMinutes(Math.Pow(2, AttemptCount - 1));
        }
        UpdatedAt = now;
    }

    public void ReleaseGenerationLease(Guid token, DateTimeOffset now)
    {
        EnsureActiveLease(token, now); ClearLease(); UpdatedAt = now;
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

    private void EnsureActiveLease(Guid token, DateTimeOffset now)
    {
        if (token == Guid.Empty || LeaseToken != token || LeaseExpiresAt <= now)
            throw new InvalidOperationException("Visual generation lease is missing, expired, or owned by another worker.");
    }

    private void ClearLease() { LeaseToken = null; LeaseOwner = null; LeaseExpiresAt = null; }
    private static string NormalizeFailureCode(string value)
    {
        var normalized = new string(value.Trim().ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '-').ToArray());
        return normalized[..Math.Min(normalized.Length, 80)];
    }

    private void InvalidateCandidateEvidence()
    {
        CandidateMediaAssetId = null; Provider = null; LicenseName = null; Attribution = null; CandidateAltText = null;
        TopicScore = null; TextSafetyScore = null; CropScore = null; OriginalityScore = null;
        ClosestMediaAssetId = null; ClosestSimilarityPercent = null; ReviewedByUserId = null; ReviewedAt = null;
    }
}
