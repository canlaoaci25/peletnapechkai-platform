namespace Peletnapechkai.Api.Domain.Content;

public sealed record ContentAuthorityAssessment(int Score, string[] Risks);

public static class ContentAuthorityPolicy
{
    public static ContentAuthorityAssessment Assess(IEnumerable<string> sourceUrls, bool hasSeo, bool hasCover, int categoryCount, int tagCount)
    {
        var urls = sourceUrls.Select(value => Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null).Where(uri => uri is not null).Cast<Uri>().ToArray();
        var domains = urls.Select(uri => uri.Host.ToLowerInvariant()).Distinct().Count();
        var risks = new List<string>();
        if (urls.Length == 0) risks.Add("missing_sources");
        else if (urls.Length == 1) risks.Add("single_source");
        else if (domains == 1) risks.Add("single_domain");
        if (urls.Any(uri => uri.Scheme != Uri.UriSchemeHttps)) risks.Add("insecure_source");
        if (!hasSeo) risks.Add("missing_seo");
        if (!hasCover) risks.Add("missing_cover");
        if (categoryCount == 0) risks.Add("missing_category");
        if (tagCount == 0) risks.Add("missing_tags");
        var score = 100 - risks.Sum(risk => risk switch { "missing_sources" => 35, "single_source" => 15, "single_domain" => 10, "insecure_source" => 10, "missing_seo" => 15, "missing_cover" => 10, "missing_category" => 10, "missing_tags" => 5, _ => 0 });
        return new(Math.Max(0, score), risks.ToArray());
    }
}
