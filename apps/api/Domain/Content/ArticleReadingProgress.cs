using Peletnapechkai.Api.Domain.Identity;

namespace Peletnapechkai.Api.Domain.Content;

public sealed class ArticleReadingProgress
{
    private ArticleReadingProgress() { }

    public ArticleReadingProgress(ApplicationUser user, ArticleLocalization article, int percent, string? anchor, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7(); User = user; UserId = user.Id; ArticleLocalization = article;
        ArticleLocalizationId = article.Id; Update(percent, anchor, now);
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization ArticleLocalization { get; private set; } = null!;
    public int Percent { get; private set; }
    public string? Anchor { get; private set; }
    public DateTimeOffset LastReadAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void Update(int percent, string? anchor, DateTimeOffset now)
    {
        var boundedPercent = Math.Clamp(percent, 0, 100);
        Percent = CompletedAt.HasValue ? Math.Max(Percent, boundedPercent) : boundedPercent;
        Anchor = string.IsNullOrWhiteSpace(anchor) ? null : anchor.Trim()[..Math.Min(anchor.Trim().Length, 160)];
        LastReadAt = now;
        if (boundedPercent >= 95 && !CompletedAt.HasValue) CompletedAt = now;
    }
}
