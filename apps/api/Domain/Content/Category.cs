using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Domain.Content;

public sealed class Category
{
    private Category() { }

    public Category(Locale locale, string slug, string name, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(locale);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = Guid.CreateVersion7();
        Locale = locale;
        LocaleId = locale.Id;
        Slug = slug.Trim();
        Name = name.Trim();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid LocaleId { get; private set; }
    public Locale Locale { get; private set; } = null!;
    public string Slug { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public ICollection<ArticleLocalization> Articles { get; } = [];
}
