namespace Peletnapechkai.Api.Domain.Content;

public sealed class ArticleEngagement
{
    private ArticleEngagement() { }
    public ArticleEngagement(ArticleLocalization article, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7(); ArticleLocalization = article; ArticleLocalizationId = article.Id; LastViewedAt = now;
    }
    public Guid Id { get; private set; }
    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization ArticleLocalization { get; private set; } = null!;
    public long ViewCount { get; private set; }
    public long EngagedSeconds { get; private set; }
    public DateTimeOffset LastViewedAt { get; private set; }
    public void RecordView(DateTimeOffset now) { ViewCount++; LastViewedAt = now; }
    public void RecordEngagement(int seconds, DateTimeOffset now) { EngagedSeconds += Math.Clamp(seconds, 0, 300); LastViewedAt = now; }
}
