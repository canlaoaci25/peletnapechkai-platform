using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class HomepageEndpoints
{
    public static IEndpointRouteBuilder MapHomepageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var publicGroup = endpoints.MapGroup("/api/v1/public").WithTags("Homepage");
        publicGroup.MapGet("/{locale}/homepage", GetHomepageAsync);
        publicGroup.MapPost("/{locale}/articles/{slug}/engagement", RecordEngagementAsync);
        var admin = endpoints.MapGroup("/api/v1/admin/homepage").WithTags("Homepage management").RequireAuthorization(AuthorizationPolicies.ManageEditorial);
        admin.MapGet("/{locale}", GetAdminAsync);
        admin.MapPut("/{locale}", SaveAsync).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> GetHomepageAsync(string locale, PublishingDbContext db, CancellationToken token)
    {
        var articles = await Candidates(locale, db, token);
        if (articles.Count == 0) return Results.Ok(new { lead=(object?)null, secondary=Array.Empty<object>(), trending=Array.Empty<object>(), editors=Array.Empty<object>(), latest=Array.Empty<object>() });
        var placements = await db.HomepagePlacements.AsNoTracking().Where(x => x.Locale.Code == locale).OrderBy(x => x.Position).Select(x => new { x.Section, x.ArticleLocalizationId }).ToListAsync(token);
        var byId = articles.ToDictionary(x => x.Id);
        var manualLead = placements.FirstOrDefault(x => x.Section == "Lead" && byId.ContainsKey(x.ArticleLocalizationId));
        var lead = manualLead is null ? articles[0] : byId[manualLead.ArticleLocalizationId];
        var automatic = articles.OrderByDescending(Score).ThenByDescending(x => x.PublishedAt).ToList();
        var editors = placements.Where(x => x.Section == "Editors" && byId.ContainsKey(x.ArticleLocalizationId)).Select(x => byId[x.ArticleLocalizationId]).ToList();
        editors.AddRange(automatic.Where(x => x.Id != lead.Id && editors.All(y => y.Id != x.Id)).Take(4-editors.Count));
        var secondary = articles.Where(x => x.Id != lead.Id).Take(4).ToList();
        var excluded = new HashSet<Guid>(secondary.Select(x=>x.Id)) { lead.Id };
        return Results.Ok(new { lead=Shape(lead), secondary=secondary.Select(Shape), trending=automatic.Take(6).Select(Shape), editors=editors.Take(4).Select(Shape), latest=articles.Where(x=>!excluded.Contains(x.Id)).Take(15).Select(Shape), mode=placements.Count==0?"Automatic":"Hybrid" });
    }

    private static double Score(Candidate x)
    {
        var ageHours=Math.Max(1,(DateTimeOffset.UtcNow-(x.PublishedAt??x.UpdatedAt)).TotalHours);
        return Math.Log10(x.Views+1)*35 + Math.Log10(x.EngagedSeconds+1)*12 + 120/Math.Pow(ageHours+2,.55);
    }

    private static object Shape(Candidate x) => new { articleGroupId=x.GroupId, slug=x.Slug, title=x.Title, summary=x.Summary, type=x.Type, publishedAt=x.PublishedAt, updatedAt=x.UpdatedAt, cover=x.CoverId is null?null:new { url="/api/media/"+x.CoverId+"?v="+x.CoverBytes, altText=x.CoverAlt }, views=x.Views };

    private static async Task<List<Candidate>> Candidates(string locale, PublishingDbContext db, CancellationToken token) => await db.ArticleLocalizations.AsNoTracking()
        .Where(x=>x.Locale.Code==locale&&x.Locale.IsEnabled&&x.Status==PublicationStatus.Published)
        .OrderByDescending(x=>x.PublishedAt).Take(50)
        .Select(x=>new Candidate(x.Id,x.ArticleGroupId,x.Slug,x.Title,x.Summary,x.ArticleGroup.Type.ToString(),x.PublishedAt,x.UpdatedAt,x.CoverMediaAssetId,x.CoverAltText,x.CoverMediaAsset==null?0:x.CoverMediaAsset.OptimizedByteLength??x.CoverMediaAsset.ByteLength,db.ArticleEngagements.Where(e=>e.ArticleLocalizationId==x.Id).Select(e=>e.ViewCount).FirstOrDefault(),db.ArticleEngagements.Where(e=>e.ArticleLocalizationId==x.Id).Select(e=>e.EngagedSeconds).FirstOrDefault())).ToListAsync(token);

    private static async Task<IResult> RecordEngagementAsync(string locale,string slug,EngagementRequest request,PublishingDbContext db,CancellationToken token)
    {
        var article=await db.ArticleLocalizations.SingleOrDefaultAsync(x=>x.Locale.Code==locale&&x.Slug==slug&&x.Status==PublicationStatus.Published,token); if(article is null)return Results.NotFound();
        var metric=await db.ArticleEngagements.SingleOrDefaultAsync(x=>x.ArticleLocalizationId==article.Id,token); if(metric is null){metric=new ArticleEngagement(article,DateTimeOffset.UtcNow);db.ArticleEngagements.Add(metric);}
        if(request.Kind=="view")metric.RecordView(DateTimeOffset.UtcNow);else if(request.Kind=="engaged")metric.RecordEngagement(request.Seconds,DateTimeOffset.UtcNow);else return Results.BadRequest();
        await db.SaveChangesAsync(token);return Results.NoContent();
    }

    private static async Task<IResult> GetAdminAsync(string locale,PublishingDbContext db,CancellationToken token)
    {
        var articles=await Candidates(locale,db,token);var placements=await db.HomepagePlacements.AsNoTracking().Where(x=>x.Locale.Code==locale).OrderBy(x=>x.Section).ThenBy(x=>x.Position).Select(x=>new{x.Section,x.Position,x.ArticleLocalizationId}).ToListAsync(token);
        return Results.Ok(new{mode=placements.Count==0?"Automatic":"Hybrid",placements,articles=articles.Select(x=>new{x.Id,x.Title,x.Type,x.PublishedAt,x.Views,score=Math.Round(Score(x),2)})});
    }

    private static async Task<IResult> SaveAsync(string locale,HomepageRequest request,PublishingDbContext db,CancellationToken token)
    {
        var localeEntity=await db.Locales.SingleOrDefaultAsync(x=>x.Code==locale&&x.IsEnabled,token);if(localeEntity is null)return Results.NotFound();
        var old=await db.HomepagePlacements.Where(x=>x.LocaleId==localeEntity.Id).ToListAsync(token);db.HomepagePlacements.RemoveRange(old);
        if(!request.AutomaticOnly){var ids=request.Placements.Select(x=>x.ArticleId).Distinct().ToArray();var articles=await db.ArticleLocalizations.Where(x=>ids.Contains(x.Id)&&x.LocaleId==localeEntity.Id&&x.Status==PublicationStatus.Published).ToDictionaryAsync(x=>x.Id,token);if(articles.Count!=ids.Length)return Results.BadRequest(new{message="All placements must reference published articles in this locale."});foreach(var item in request.Placements.Take(5)){if(item.Section is not("Lead" or "Editors"))return Results.BadRequest();db.HomepagePlacements.Add(new HomepagePlacement(localeEntity,articles[item.ArticleId],item.Section,item.Position,DateTimeOffset.UtcNow));}}
        await db.SaveChangesAsync(token);return Results.NoContent();
    }

    private sealed record Candidate(Guid Id,Guid GroupId,string Slug,string Title,string Summary,string Type,DateTimeOffset? PublishedAt,DateTimeOffset UpdatedAt,Guid? CoverId,string? CoverAlt,long CoverBytes,long Views,long EngagedSeconds);
    public sealed record EngagementRequest(string Kind,int Seconds=0);
    public sealed record HomepagePlacementRequest(string Section,int Position,Guid ArticleId);
    public sealed record HomepageRequest(bool AutomaticOnly,HomepagePlacementRequest[] Placements);
}
