using Peletnapechkai.Api.Domain.Content;

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
                !Source.TryNormalizePublicUrl(uri, out var canonical))
            {
                return false;
            }

            if (!canonicalUrls.Add(canonical)) return false;
            hosts.Add(uri.IdnHost.TrimEnd('.'));
        }

        return hosts.Count >= 2;
    }
}
