using Peletnapechkai.Api.Domain.Identity;

namespace Peletnapechkai.Api.Domain.Content;

public sealed class SavedArticle
{
    private SavedArticle() { }

    public SavedArticle(ApplicationUser user, ArticleLocalization article, DateTimeOffset savedAt)
    {
        Id = Guid.CreateVersion7();
        User = user;
        UserId = user.Id;
        ArticleLocalization = article;
        ArticleLocalizationId = article.Id;
        SavedAt = savedAt;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ApplicationUser User { get; private set; } = null!;
    public Guid ArticleLocalizationId { get; private set; }
    public ArticleLocalization ArticleLocalization { get; private set; } = null!;
    public DateTimeOffset SavedAt { get; private set; }
}
