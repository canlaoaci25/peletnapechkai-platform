namespace Peletnapechkai.Api.Domain.Content;

using System.Net;

public sealed class Source
{
    private Source() { }

    public Source(string name, Uri url, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(url);
        if (!TryNormalizePublicUrl(url, out var normalizedUrl))
        {
            throw new ArgumentException("Source URL must be a public HTTP or HTTPS URL.", nameof(url));
        }

        Id = Guid.CreateVersion7();
        Name = name.Trim();
        Url = normalizedUrl;
        CreatedAt = createdAt;
    }

    public static bool TryNormalizePublicUrl(Uri? url, out string normalizedUrl)
    {
        normalizedUrl = string.Empty;
        if (url is null || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps) ||
            !string.IsNullOrEmpty(url.UserInfo) || !IsPublicHost(url.Host))
        {
            return false;
        }

        normalizedUrl = new UriBuilder(url) { Fragment = string.Empty }.Uri.AbsoluteUri;
        return true;
    }

    private static bool IsPublicHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)) return false;
        if (!IPAddress.TryParse(host, out var address)) return host.Contains('.');
        if (IPAddress.IsLoopback(address)) return false;
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return bytes[0] != 10 && bytes[0] != 127 && bytes[0] != 0 && bytes[0] < 224 &&
                   !(bytes[0] == 169 && bytes[1] == 254) && !(bytes[0] == 172 && bytes[1] is >= 16 and <= 31) &&
                   !(bytes[0] == 192 && bytes[1] == 168);
        }
        return !address.Equals(IPAddress.IPv6Any) && !address.IsIPv6LinkLocal && !address.IsIPv6Multicast &&
               !address.IsIPv6SiteLocal && !(bytes[0] is 0xfc or 0xfd);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
