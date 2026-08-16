using System.Net;

namespace Peletnapechkai.Api.Infrastructure.Automation;

public static class GeneratedSourceQualityPolicy
{
    public static bool IsValid(IEnumerable<(string? Name, string? Url)> sources)
    {
        var items = sources.ToArray();
        if (items.Length is < 2 or > 8) return false;

        var canonicalUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Name.Trim().Length > 200 ||
                !Uri.TryCreate(item.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                !string.IsNullOrEmpty(uri.UserInfo) || !IsPublicHost(uri.Host))
            {
                return false;
            }

            var builder = new UriBuilder(uri) { Fragment = string.Empty };
            if (!canonicalUrls.Add(builder.Uri.AbsoluteUri)) return false;
            hosts.Add(uri.IdnHost.TrimEnd('.'));
        }

        return hosts.Count >= 2;
    }

    private static bool IsPublicHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!IPAddress.TryParse(host, out var address)) return host.Contains('.');
        if (IPAddress.IsLoopback(address)) return false;
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();

        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return bytes[0] != 10 && bytes[0] != 127 &&
                   !(bytes[0] == 169 && bytes[1] == 254) &&
                   !(bytes[0] == 172 && bytes[1] is >= 16 and <= 31) &&
                   !(bytes[0] == 192 && bytes[1] == 168) &&
                   !(bytes[0] == 0) && !(bytes[0] >= 224);
        }

        return !address.Equals(IPAddress.IPv6Any) && !address.IsIPv6LinkLocal && !address.IsIPv6Multicast && !address.IsIPv6SiteLocal &&
               !(bytes[0] is 0xfc or 0xfd);
    }
}
