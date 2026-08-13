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
                TagCount = x.Tags.Count,
                HasSeo = x.SeoTitle != null && x.SeoDescription != null,
                HasCover = x.CoverMediaAssetId != null
            }).ToListAsync(token);
        var totalViews = rows.Sum(x => x.Views); var totalSeconds = rows.Sum(x => x.EngagedSeconds);
        var opportunities = rows.OrderBy(x => x.Views).ThenByDescending(x => x.PublishedAt).Take(20)
            .Select(x => new { x.Id, x.Slug, x.Title, x.Views, x.EngagedSeconds, x.HasSeo, x.HasCover, x.TagCount,
                reason = !x.HasSeo ? "SEO bilgisi eksik" : !x.HasCover ? "Kapak görseli eksik" : x.TagCount == 0 ? "Etiket ve konu bağlantısı eksik" : "Düşük keşif: iç bağlantı ve dağıtım adayı" });
        var clusters = rows.SelectMany(x => x.Categories.DefaultIfEmpty("Kategorisiz").Select(category => new { category, x.Views, x.EngagedSeconds }))
            .GroupBy(x => x.category).Select(x => new { name=x.Key, articles=x.Count(), views=x.Sum(y=>y.Views), engagedSeconds=x.Sum(y=>y.EngagedSeconds) })
            .OrderByDescending(x => x.views).ThenByDescending(x => x.articles).Take(12);
        var searchConsole = await GetSearchConsoleAsync(configuration, token);
        return Results.Ok(new { locale, checkedAt=DateTimeOffset.UtcNow, published=rows.Count, totalViews, totalEngagedSeconds=totalSeconds,
            averageEngagedSeconds=rows.Count == 0 ? 0 : Math.Round((double)totalSeconds/rows.Count,1),
            measurement=new { internalAnalytics=true, ga4=!string.IsNullOrWhiteSpace(configuration["Analytics:GaMeasurementId"]), clarity=!string.IsNullOrWhiteSpace(configuration["Analytics:ClarityProjectId"]), searchConsole=searchConsole is not null },
            searchConsole, top=rows.OrderByDescending(x=>x.Views).ThenByDescending(x=>x.EngagedSeconds).Take(10).Select(x=>new{x.Slug,x.Title,x.Views,x.EngagedSeconds}), opportunities, clusters });
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
