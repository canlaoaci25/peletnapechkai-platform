using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Domain.Knowledge;

public enum KnowledgeUsePurpose { Evidence, Background, UpdatePrompt }

public sealed class KnowledgeArticleLink
{
    private KnowledgeArticleLink() { }

    public KnowledgeArticleLink(KnowledgeCandidate candidate, ArticleLocalization article, KnowledgeUsePurpose purpose, string? note, DateTimeOffset reviewDueAt, Guid createdByUserId, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(candidate); ArgumentNullException.ThrowIfNull(article);
        if (candidate.Status != KnowledgeReviewStatus.Approved) throw new InvalidOperationException("Only approved knowledge can be linked.");
        if (candidate.LocaleId != article.LocaleId) throw new InvalidOperationException("Knowledge and article locales must match.");
        if (reviewDueAt <= now) throw new ArgumentOutOfRangeException(nameof(reviewDueAt), "Review date must be in the future.");
        Id=Guid.CreateVersion7(); Candidate=candidate; KnowledgeCandidateId=candidate.Id; Article=article; ArticleLocalizationId=article.Id; Purpose=purpose;
        Note=string.IsNullOrWhiteSpace(note)?null:note.Trim(); ReviewDueAt=reviewDueAt; CreatedByUserId=createdByUserId; CreatedAt=now; LastVerifiedAt=now; LastVerifiedByUserId=createdByUserId;
    }

    public Guid Id { get; private set; }
    public Guid KnowledgeCandidateId { get; private set; }
    public KnowledgeCandidate Candidate { get; private set; } = null!;
    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization Article { get; private set; } = null!;
    public KnowledgeUsePurpose Purpose { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset ReviewDueAt { get; private set; }
    public DateTimeOffset LastVerifiedAt { get; private set; }
    public Guid LastVerifiedByUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void Verify(DateTimeOffset nextReviewDueAt, Guid reviewerUserId, DateTimeOffset now)
    {
        if (nextReviewDueAt <= now) throw new ArgumentOutOfRangeException(nameof(nextReviewDueAt));
        LastVerifiedAt=now; LastVerifiedByUserId=reviewerUserId; ReviewDueAt=nextReviewDueAt;
    }
}
