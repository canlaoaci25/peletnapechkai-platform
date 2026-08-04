namespace Peletnapechkai.Api.Domain.Content;

public sealed class ArticleGroup
{
    private ArticleGroup()
    {
    }

    public ArticleGroup(ArticleType type, DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7();
        Type = type;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public ArticleType Type { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<ArticleLocalization> Localizations { get; } = [];

    public ICollection<Author> Authors { get; } = [];

    public ICollection<Source> Sources { get; } = [];

    public ICollection<MediaAsset> MediaAssets { get; } = [];
}
