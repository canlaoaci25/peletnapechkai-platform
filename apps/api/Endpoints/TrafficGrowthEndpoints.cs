using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class TrafficGrowthEndpoints
{
    public static IEndpointRouteBuilder MapTrafficGrowthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/admin/traffic/{locale}", GetDashboardAsync)
            .RequireAuthorization(AuthorizationPolicies.ManageUsers).WithTags("Traffic growth");
        endpoints.MapGet("/api/v1/public/{locale}/articles/{slug}/related", GetRelatedAsync)
            .WithTags("Traffic growth");
        return endpoints;
    }

    private static async Task<IResult> GetDashboardAsync(string locale, PublishingDbContext db, IConfiguration configuration, CancellationToken token)
    {
        var rows = await db.ArticleLocalizations.AsNoTracking()
            .Where(x => x.Locale.Code == locale && x.Status == PublicationStatus.Published)
            .Select(x => new
            {
                x.Id, x.Slug, x.Title, x.PublishedAt, x.UpdatedAt,
                Views = db.ArticleEngagements.Where(e => e.ArticleLocalizationId == x.Id).Select(e => e.ViewCount).FirstOrDefault(),
                EngagedSeconds = db.ArticleEngagements.Where(e => e.ArticleLocalizationId == x.Id).Select(e => e.EngagedSeconds).FirstOrDefault(),
                Categories = x.Categories.Select(c => c.Name).ToArray(),
                CategoryCount = x.Categories.Count,
                TagCount = x.Tags.Count,
                SourceUrls = x.ArticleGroup.Sources.Select(source => source.Url).ToArray(),
                HasSeo = x.SeoTitle != null && x.SeoDescription != null,
                HasCover = x.CoverMediaAssetId != null
            }).ToListAsync(token);
        var totalViews = rows.Sum(x => x.Views); var totalSeconds = rows.Sum(x => x.EngagedSeconds);
        var assessed = rows.Select(x => new { Row = x, Authority = ContentAuthorityPolicy.Assess(x.SourceUrls, x.HasSeo, x.HasCover, x.CategoryCount, x.TagCount) }).ToArray();
        var opportunities = assessed.Where(x => x.Authority.Risks.Length > 0)
            .OrderBy(x => x.Authority.Score).ThenByDescending(x => x.Row.Views).ThenByDescending(x => x.Row.PublishedAt).Take(24)
            .Select(x => new { x.Row.Id, x.Row.Slug, x.Row.Title, x.Row.Views, x.Row.EngagedSeconds, x.Row.HasSeo, x.Row.HasCover, x.Row.TagCount,
                sourceCount=x.Row.SourceUrls.Length, domainCount=x.Row.SourceUrls.Select(url=>new Uri(url).Host.ToLowerInvariant()).Distinct().Count(), authorityScore=x.Authority.Score, risks=x.Authority.Risks });
        var sourceDomains = rows.SelectMany(x => x.SourceUrls.Select(url => new { x.Id, Domain = new Uri(url).Host.ToLowerInvariant() }))
            .GroupBy(x => x.Domain).Select(group => new { domain=group.Key, articles=group.Select(x=>x.Id).Distinct().Count(), citations=group.Count() })
            .OrderByDescending(x=>x.articles).ThenBy(x=>x.domain).Take(12).ToArray();
        var authority = new { strong=assessed.Count(x=>x.Authority.Score>=80), needsWork=assessed.Count(x=>x.Authority.Score is >=50 and <80), critical=assessed.Count(x=>x.Authority.Score<50),
            averageScore=assessed.Length==0?0:Math.Round(assessed.Average(x=>x.Authority.Score),1), withoutSources=assessed.Count(x=>x.Authority.Risks.Contains("missing_sources")), singleSource=assessed.Count(x=>x.Authority.Risks.Contains("single_source")) };
        var clusters = rows.SelectMany(x => x.Categories.DefaultIfEmpty("Kategorisiz").Select(category => new { category, x.Views, x.EngagedSeconds }))
            .GroupBy(x => x.category).Select(x => new { name=x.Key, articles=x.Count(), views=x.Sum(y=>y.Views), engagedSeconds=x.Sum(y=>y.EngagedSeconds) })
            .OrderByDescending(x => x.views).ThenByDescending(x => x.articles).Take(12);
        var searchConsole = await GetSearchConsoleAsync(configuration, token);
        return Results.Ok(new { locale, checkedAt=DateTimeOffset.UtcNow, published=rows.Count, totalViews, totalEngagedSeconds=totalSeconds,
            averageEngagedSeconds=rows.Count == 0 ? 0 : Math.Round((double)totalSeconds/rows.Count,1),
            measurement=new { internalAnalytics=true, ga4=!string.IsNullOrWhiteSpace(configuration["Analytics:GaMeasurementId"]), clarity=!string.IsNullOrWhiteSpace(configuration["Analytics:ClarityProjectId"]), searchConsole=searchConsole is not null },
            searchConsole, authority, sourceDomains, top=rows.OrderByDescending(x=>x.Views).ThenByDescending(x=>x.EngagedSeconds).Take(10).Select(x=>new{x.Slug,x.Title,x.Views,x.EngagedSeconds}), opportunities, clusters });
    }

    private static async Task<object?> GetSearchConsoleAsync(IConfiguration configuration, CancellationToken token)
    {
        var root = configuration["SearchConsole:CredentialPath"] ?? @"C:\ProgramData\Peletnapechkai\SearchConsole";
        var clientPath = Path.Combine(root, "oauth-client.json"); var tokenPath = Path.Combine(root, "oauth-token.json");
        if (!File.Exists(clientPath) || !File.Exists(tokenPath)) return null;
        try
        {
            using var clientDocument = JsonDocument.Parse(await File.ReadAllTextAsync(clientPath, token));
            using var tokenDocument = JsonDocument.Parse(await File.ReadAllTextAsync(tokenPath, token));
            var installed = clientDocument.RootElement.GetProperty("installed");
            var form = new Dictionary<string,string> { ["client_id"]=installed.GetProperty("client_id").GetString()!, ["client_secret"]=installed.GetProperty("client_secret").GetString()!, ["refresh_token"]=tokenDocument.RootElement.GetProperty("refresh_token").GetString()!, ["grant_type"]="refresh_token" };
            using var http = new HttpClient { Timeout=TimeSpan.FromSeconds(10) };
            using var tokenResponse = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(form), token); tokenResponse.EnsureSuccessStatusCode();
            using var accessDocument = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(token));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessDocument.RootElement.GetProperty("access_token").GetString());
            var end = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)); var start=end.AddDays(-27);
            var request = JsonSerializer.Serialize(new { startDate=start.ToString("yyyy-MM-dd"), endDate=end.ToString("yyyy-MM-dd"), dimensions=new[]{"query"}, rowLimit=10 });
            var site = Uri.EscapeDataString(configuration["SearchConsole:SiteUrl"] ?? "https://peletnapechkai.com/");
            using var response = await http.PostAsync($"https://www.googleapis.com/webmasters/v3/sites/{site}/searchAnalytics/query", new StringContent(request,System.Text.Encoding.UTF8,"application/json"), token); response.EnsureSuccessStatusCode();
            using var data = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
            var rows = data.RootElement.TryGetProperty("rows",out var resultRows) ? resultRows.EnumerateArray().Select(row=>new { query=row.GetProperty("keys")[0].GetString(), clicks=row.GetProperty("clicks").GetDouble(), impressions=row.GetProperty("impressions").GetDouble(), ctr=row.GetProperty("ctr").GetDouble(), position=row.GetProperty("position").GetDouble() }).ToArray() : [];
            return new { startDate=start, endDate=end, clicks=rows.Sum(x=>x.clicks), impressions=rows.Sum(x=>x.impressions), ctr=rows.Sum(x=>x.impressions)==0?0:rows.Sum(x=>x.clicks)/rows.Sum(x=>x.impressions), averagePosition=rows.Length==0?0:rows.Average(x=>x.position), queries=rows };
        }
        catch { return null; }
    }

    private static async Task<IResult> GetRelatedAsync(string locale, string slug, PublishingDbContext db, CancellationToken token)
    {
        var source = await db.ArticleLocalizations.AsNoTracking().Where(x => x.Locale.Code == locale && x.Slug == slug && x.Status == PublicationStatus.Published)
            .Select(x => new { x.Id, CategoryIds=x.Categories.Select(c=>c.Id).ToArray(), TagIds=x.Tags.Select(t=>t.Id).ToArray() }).SingleOrDefaultAsync(token);
        if (source is null) return Results.NotFound();
        var items = await db.ArticleLocalizations.AsNoTracking().Where(x => x.Id != source.Id && x.Locale.Code == locale && x.Status == PublicationStatus.Published)
            .Select(x => new { x.Slug,x.Title,x.Summary,type=x.ArticleGroup.Type.ToString(),x.PublishedAt,x.UpdatedAt,
                Score=x.Categories.Count(c=>source.CategoryIds.Contains(c.Id))*4+x.Tags.Count(t=>source.TagIds.Contains(t.Id))*2,
                cover=x.CoverMediaAssetId==null?null:new{url="/api/media/"+x.CoverMediaAssetId+"?v="+x.CoverMediaAsset!.OptimizedByteLength,altText=x.CoverAltText} })
            .OrderByDescending(x=>x.Score).ThenByDescending(x=>x.PublishedAt).Take(4).ToListAsync(token);
        return Results.Ok(items);
    }
}
