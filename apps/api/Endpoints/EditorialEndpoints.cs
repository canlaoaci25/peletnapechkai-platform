using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class EditorialEndpoints
{
    public static IEndpointRouteBuilder MapEditorialEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/articles").WithTags("Editorial").RequireAuthorization();
        group.MapGet("/", ListAsync).RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapGet("/{articleId:guid}", GetAsync).RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapGet("/{articleId:guid}/revisions", ListRevisionsAsync).RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapPost("/", CreateAsync).RequireAuthorization(AuthorizationPolicies.WriteContent).ValidateAntiforgery();
        group.MapPut("/{articleId:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.WriteContent).ValidateAntiforgery();
        group.MapPut("/{articleId:guid}/relationships", UpdateRelationshipsAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/submit", SubmitAsync).RequireAuthorization(AuthorizationPolicies.WriteContent).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/editorial-approve", EditorialApproveAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/return-to-draft", ReturnToDraftAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/schedule", ScheduleAsync).RequireAuthorization(AuthorizationPolicies.ManageSeo).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/publish", PublishAsync).RequireAuthorization(AuthorizationPolicies.ManageSeo).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/archive", ArchiveAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> ListRevisionsAsync(Guid articleId, PublishingDbContext database, CancellationToken token)
    {
        var exists = await database.ArticleLocalizations.AsNoTracking().AnyAsync(item => item.Id == articleId, token);
        if (!exists) return Results.NotFound();
        var revisions = await database.ArticleRevisions.AsNoTracking().Where(item => item.ArticleLocalizationId == articleId)
            .OrderByDescending(item => item.Number).Take(50)
            .Select(item => new { item.Id, item.Number, item.Title, item.Summary, item.Body, item.CreatedByUserId, item.CreatedAt }).ToListAsync(token);
        return Results.Ok(revisions);
    }

    private static async Task<IResult> ListAsync(PublishingDbContext database, string? status, string? locale, CancellationToken token)
    {
        var query = database.ArticleLocalizations.AsNoTracking().Include(x => x.Locale).AsQueryable();
        if (Enum.TryParse<PublicationStatus>(status, true, out var parsedStatus)) query = query.Where(x => x.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(locale)) query = query.Where(x => x.Locale.Code == locale);
        var articles = await query.OrderByDescending(x => x.UpdatedAt).Take(100).Select(x => new
        {
            x.Id,
            x.ArticleGroupId,
            locale = x.Locale.Code,
            type = x.ArticleGroup.Type.ToString(),
            x.Slug,
            x.Title,
            status = x.Status.ToString(),
            x.UpdatedAt,
            x.ScheduledAt,
            x.PublishedAt
        }).ToListAsync(token);
        return Results.Ok(articles);
    }

    private static async Task<IResult> GetAsync(Guid articleId, PublishingDbContext database, CancellationToken token)
    {
        var article = await database.ArticleLocalizations.AsNoTracking().Include(x => x.Locale).Where(x => x.Id == articleId)
            .Select(x => new { x.Id, x.ArticleGroupId, locale = x.Locale.Code, type = x.ArticleGroup.Type.ToString(), x.Slug, x.Title, x.Summary, x.Body, x.SeoTitle, x.SeoDescription, x.IsSponsored, x.SponsorName, x.HasAffiliateLinks, x.CoverMediaAssetId, x.CoverAltText, x.CoverCaption, x.CoverCredit, status = x.Status.ToString(), x.UpdatedAt, x.ScheduledAt, x.PublishedAt, categoryIds=x.Categories.Select(item=>item.Id), tagIds=x.Tags.Select(item=>item.Id), authorIds=x.ArticleGroup.Authors.Select(item=>item.Id), sourceIds=x.ArticleGroup.Sources.Select(item=>item.Id), mediaAssetIds=x.ArticleGroup.MediaAssets.Select(item=>item.Id) }).SingleOrDefaultAsync(token);
        return article is null ? Results.NotFound() : Results.Ok(article);
    }

    private static async Task<IResult> CreateAsync(CreateArticleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        if (!Enum.TryParse<ArticleType>(request.Type, true, out var type)) return Validation("type", "A valid article type is required.");
        var locale = await database.Locales.SingleOrDefaultAsync(x => x.Code == request.Locale && x.IsEnabled, token);
        var actor = await users.GetUserAsync(principal);
        if (locale is null || actor is null) return Results.BadRequest();
        var now = DateTimeOffset.UtcNow;
        var articleGroup = new ArticleGroup(type, now);
        var article = new ArticleLocalization(articleGroup, locale, request.Slug, request.Title, request.Summary ?? string.Empty, request.Body ?? string.Empty, now);
        article.UpdateDraft(request.Slug, request.Title, request.Summary ?? string.Empty, request.Body ?? string.Empty, request.SeoTitle, request.SeoDescription, now);
        article.UpdateCommercialDisclosure(request.IsSponsored, request.SponsorName, request.HasAffiliateLinks, now);
        database.ArticleGroups.Add(articleGroup);
        database.AuditLogs.Add(Audit(actor.Id, "editorial.article_created", article.Id, new { request.Locale, type }));
        await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/articles/{article.Id}", new { article.Id, article.ArticleGroupId, article.UpdatedAt });
    }

    private static async Task<IResult> UpdateAsync(Guid articleId, UpdateArticleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var article = await database.ArticleLocalizations.Include(x => x.Revisions).SingleOrDefaultAsync(x => x.Id == articleId, token);
        var actor = await users.GetUserAsync(principal);
        if (article is null || actor is null) return Results.NotFound();
        if (article.UpdatedAt != request.ExpectedUpdatedAt) return Results.Conflict(new { message = "Article changed since it was loaded." });
        if (article.Status != PublicationStatus.Draft) return Results.Conflict(new { message = "Only draft articles can be edited." });
        var now = DateTimeOffset.UtcNow;
        var revision = new ArticleRevision(article, article.Revisions.Count == 0 ? 1 : article.Revisions.Max(x => x.Number) + 1, article.Title, article.Summary, article.Body, actor.Id, now);
        database.ArticleRevisions.Add(revision);
        article.UpdateDraft(request.Slug, request.Title, request.Summary ?? string.Empty, request.Body ?? string.Empty, request.SeoTitle, request.SeoDescription, now);
        article.UpdateCommercialDisclosure(request.IsSponsored, request.SponsorName, request.HasAffiliateLinks, now);
        database.AuditLogs.Add(Audit(actor.Id, "editorial.article_updated", article.Id, null));
        await database.SaveChangesAsync(token);
        return Results.Ok(new { article.Id, article.UpdatedAt, revision = revision.Number });
    }

    private static Task<IResult> SubmitAsync(Guid articleId, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token) =>
        TransitionAsync(articleId, "editorial.submitted", principal, users, database, x => x.SubmitForEditorialReview(DateTimeOffset.UtcNow), token);
    private static Task<IResult> EditorialApproveAsync(Guid articleId, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token) =>
        TransitionAsync(articleId, "editorial.approved", principal, users, database, x => x.ApproveEditorialReview(DateTimeOffset.UtcNow), token);
    private static Task<IResult> ReturnToDraftAsync(Guid articleId, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token) =>
        TransitionAsync(articleId, "editorial.returned_to_draft", principal, users, database, x => x.ReturnToDraft(DateTimeOffset.UtcNow), token);
    private static Task<IResult> PublishAsync(Guid articleId, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token) =>
        TransitionAsync(articleId, "editorial.published", principal, users, database, x => x.Publish(DateTimeOffset.UtcNow), token);
    private static Task<IResult> ArchiveAsync(Guid articleId, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token) =>
        TransitionAsync(articleId, "editorial.archived", principal, users, database, x => x.Archive(DateTimeOffset.UtcNow), token);

    private static Task<IResult> ScheduleAsync(Guid articleId, ScheduleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token) =>
        TransitionAsync(articleId, "editorial.scheduled", principal, users, database, x => x.Schedule(request.ScheduledAt, DateTimeOffset.UtcNow), token);

    private static async Task<IResult> UpdateRelationshipsAsync(Guid articleId, RelationshipsRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var article = await database.ArticleLocalizations.Include(x=>x.Categories).Include(x=>x.Tags).Include(x=>x.ArticleGroup).ThenInclude(x=>x.Authors).Include(x=>x.ArticleGroup).ThenInclude(x=>x.Sources).Include(x=>x.ArticleGroup).ThenInclude(x=>x.MediaAssets).SingleOrDefaultAsync(x=>x.Id==articleId,token);
        var actor=await users.GetUserAsync(principal);
        if(article is null || actor is null)return Results.NotFound();
        if(article.Status!=PublicationStatus.Draft)return Results.Conflict(new {message="Only draft article relationships can be edited."});
        var categories=await database.Categories.Where(x=>request.CategoryIds.Contains(x.Id) && x.LocaleId==article.LocaleId).ToListAsync(token);
        var tags=await database.Tags.Where(x=>request.TagIds.Contains(x.Id) && x.LocaleId==article.LocaleId).ToListAsync(token);
        var authors=await database.Authors.Where(x=>request.AuthorIds.Contains(x.Id)).ToListAsync(token);
        var sources=await database.Sources.Where(x=>request.SourceIds.Contains(x.Id)).ToListAsync(token);
        var media=await database.MediaAssets.Where(x=>request.MediaAssetIds.Contains(x.Id)).ToListAsync(token);
        if(categories.Count!=request.CategoryIds.Distinct().Count() || tags.Count!=request.TagIds.Distinct().Count() || authors.Count!=request.AuthorIds.Distinct().Count() || sources.Count!=request.SourceIds.Distinct().Count() || media.Count!=request.MediaAssetIds.Distinct().Count()) return Validation("relationships","One or more relationships are invalid for this locale.");
        var cover=request.CoverMediaAssetId is null?null:media.SingleOrDefault(x=>x.Id==request.CoverMediaAssetId);
        if(request.CoverMediaAssetId is not null && cover is null)return Validation("coverMediaAssetId","The cover must be one of the selected media assets.");
        if(cover is not null && string.IsNullOrWhiteSpace(request.CoverAltText))return Validation("coverAltText","Cover alternative text is required.");
        article.Categories.Clear(); foreach(var item in categories)article.Categories.Add(item);
        article.Tags.Clear(); foreach(var item in tags)article.Tags.Add(item);
        article.ArticleGroup.Authors.Clear(); foreach(var item in authors)article.ArticleGroup.Authors.Add(item);
        article.ArticleGroup.Sources.Clear(); foreach(var item in sources)article.ArticleGroup.Sources.Add(item);
        article.ArticleGroup.MediaAssets.Clear(); foreach(var item in media)article.ArticleGroup.MediaAssets.Add(item);
        article.UpdateCover(cover,request.CoverAltText,request.CoverCaption,request.CoverCredit,DateTimeOffset.UtcNow);
        database.AuditLogs.Add(Audit(actor.Id,"editorial.relationships_updated",article.Id,new {categories=categories.Count,tags=tags.Count,authors=authors.Count,sources=sources.Count,media=media.Count}));
        await database.SaveChangesAsync(token);
        return Results.Ok(new {article.Id});
    }

    private static async Task<IResult> TransitionAsync(Guid articleId, string action, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, Action<ArticleLocalization> transition, CancellationToken token)
    {
        var article = await database.ArticleLocalizations.SingleOrDefaultAsync(x => x.Id == articleId, token);
        var actor = await users.GetUserAsync(principal);
        if (article is null || actor is null) return Results.NotFound();
        try { transition(article); }
        catch (InvalidOperationException exception) { return Results.Conflict(new { message = exception.Message }); }
        database.AuditLogs.Add(Audit(actor.Id, action, article.Id, new { status = article.Status.ToString() }));
        await database.SaveChangesAsync(token);
        return Results.Ok(new { article.Id, status = article.Status.ToString(), article.UpdatedAt, article.ScheduledAt, article.PublishedAt });
    }

    private static AuditLog Audit(Guid actorId, string action, Guid entityId, object? details) =>
        new(actorId, action, nameof(ArticleLocalization), entityId, details is null ? null : JsonSerializer.Serialize(details), DateTimeOffset.UtcNow);
    private static IResult Validation(string key, string message) => Results.ValidationProblem(new Dictionary<string, string[]> { [key] = [message] });

    private sealed record CreateArticleRequest(string Type, string Locale, string Slug, string Title, string? Summary, string? Body, string? SeoTitle, string? SeoDescription, bool IsSponsored, string? SponsorName, bool HasAffiliateLinks);
    private sealed record UpdateArticleRequest(string Slug, string Title, string? Summary, string? Body, string? SeoTitle, string? SeoDescription, bool IsSponsored, string? SponsorName, bool HasAffiliateLinks, DateTimeOffset ExpectedUpdatedAt);
    private sealed record ScheduleRequest(DateTimeOffset ScheduledAt);
    private sealed record RelationshipsRequest(Guid[] CategoryIds, Guid[] TagIds, Guid[] AuthorIds, Guid[] SourceIds, Guid[] MediaAssetIds, Guid? CoverMediaAssetId, string? CoverAltText, string? CoverCaption, string? CoverCredit);
}
