namespace Peletnapechkai.Api.Domain.Content;

public static class SourceArchivePolicy
{
    public static string? GetCanonicalDomain(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)) return null;

        var host = uri.IdnHost.TrimEnd('.').ToLowerInvariant();
        if (host.StartsWith("www.", StringComparison.Ordinal)) host = host[4..];
        return host.Length is > 0 and <= 253 ? host : null;
    }

    public static bool IsValidDomain(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 253 || value.Contains('/') || value.Contains('\\')) return false;
        return Uri.CheckHostName(value) is UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6 &&
               GetCanonicalDomain($"https://{value}") == value.ToLowerInvariant();
    }
}
