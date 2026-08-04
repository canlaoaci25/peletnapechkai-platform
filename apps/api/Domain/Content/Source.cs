namespace Peletnapechkai.Api.Domain.Content;

public sealed class Source
{
    private Source() { }

    public Source(string name, Uri url, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(url);
        if (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException("Source URL must use HTTP or HTTPS.", nameof(url));
        }

        Id = Guid.CreateVersion7();
        Name = name.Trim();
        Url = url.AbsoluteUri;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
