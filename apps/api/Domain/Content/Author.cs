namespace Peletnapechkai.Api.Domain.Content;

public sealed class Author
{
    private Author() { }

    public Author(string slug, string displayName, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Id = Guid.CreateVersion7();
        Slug = slug.Trim();
        DisplayName = displayName.Trim();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Bio { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
