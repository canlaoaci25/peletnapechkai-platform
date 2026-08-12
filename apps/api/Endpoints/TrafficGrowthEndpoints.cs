using Microsoft.EntityFrameworkCore;
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
        return Results.Ok(new { locale, checkedAt=DateTimeOffset.UtcNow, published=rows.Count, totalViews, totalEngagedSeconds=totalSeconds,
            averageEngagedSeconds=rows.Count == 0 ? 0 : Math.Round((double)totalSeconds/rows.Count,1),
            measurement=new { internalAnalytics=true, ga4=!string.IsNullOrWhiteSpace(configuration["Analytics:GaMeasurementId"]), clarity=!string.IsNullOrWhiteSpace(configuration["Analytics:ClarityProjectId"]), searchConsole=false },
            top=rows.OrderByDescending(x=>x.Views).ThenByDescending(x=>x.EngagedSeconds).Take(10).Select(x=>new{x.Slug,x.Title,x.Views,x.EngagedSeconds}), opportunities, clusters });
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
