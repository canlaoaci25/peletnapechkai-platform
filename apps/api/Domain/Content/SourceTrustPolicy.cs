namespace Peletnapechkai.Api.Domain.Content;

public static class SourceTrustPolicy
{
    public static readonly TimeSpan ReviewFreshnessWindow = TimeSpan.FromDays(365);

    public static SourceReviewState GetReviewState(SourceKind kind, DateTimeOffset? lastReviewedAt, DateTimeOffset now)
    {
        if (kind == SourceKind.Unclassified || lastReviewedAt is null) return SourceReviewState.Unclassified;
        return lastReviewedAt.Value >= now.Subtract(ReviewFreshnessWindow)
            ? SourceReviewState.Current
            : SourceReviewState.ReviewDue;
    }
}

public enum SourceReviewState { Unclassified, Current, ReviewDue }
