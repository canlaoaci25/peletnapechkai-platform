using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Peletnapechkai.Api.Infrastructure.Publishing;

namespace Peletnapechkai.Api.Endpoints;

public static partial class EditorialEndpoints
{
    public static IEndpointRouteBuilder MapEditorialEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/articles").WithTags("Editorial").RequireAuthorization();
        group.MapGet("/", ListAsync).RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapGet("/{articleId:guid}", GetAsync).RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapGet("/{articleId:guid}/revisions", ListRevisionsAsync).RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapGet("/{articleId:guid}/corrections", ListCorrectionsAsync).RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapPost("/{articleId:guid}/corrections", CreateCorrectionAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapGet("/{articleId:guid}/claim-citations", ListClaimCitationsAsync).RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapPost("/{articleId:guid}/claim-citations", CreateClaimCitationAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapDelete("/{articleId:guid}/claim-citations/{citationId:guid}", DeleteClaimCitationAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/", CreateAsync).RequireAuthorization(AuthorizationPolicies.WriteContent).ValidateAntiforgery();
        group.MapPut("/{articleId:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.WriteContent).ValidateAntiforgery();
        group.MapPut("/{articleId:guid}/relationships", UpdateRelationshipsAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/submit", SubmitAsync).RequireAuthorization(AuthorizationPolicies.WriteContent).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/editorial-approve", EditorialApproveAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/return-to-draft", ReturnToDraftAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/schedule", ScheduleAsync).RequireAuthorization(AuthorizationPolicies.ManageSeo).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/publish", PublishAsync).RequireAuthorization(AuthorizationPolicies.ManageSeo).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/publish-direct", PublishDirectAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/{articleId:guid}/archive", ArchiveAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> ListClaimCitationsAsync(Guid articleId, PublishingDbContext database, CancellationToken token)
    {
        if (!await database.ArticleLocalizations.AsNoTracking().AnyAsync(x => x.Id == articleId, token)) return Results.NotFound();
        return Results.Ok(await database.ArticleClaimCitations.AsNoTracking().Where(x => x.ArticleLocalizationId == articleId)
            .OrderBy(x => x.ApprovedAt).Select(x => new { x.Id, x.SourceId, sourceName=x.Source.Name, sourceUrl=x.Source.Url, x.Claim, x.Locator, x.ApprovedAt }).ToListAsync(token));
    }

    private static async Task<IResult> CreateClaimCitationAsync(Guid articleId, ClaimCitationRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var article=await database.ArticleLocalizations.Include(x=>x.ArticleGroup).ThenInclude(x=>x.Sources).SingleOrDefaultAsync(x=>x.Id==articleId,token);
        var actor=await users.GetUserAsync(principal);if(article is null||actor is null)return Results.NotFound();
        var source=article.ArticleGroup.Sources.SingleOrDefault(x=>x.Id==request.SourceId);
        if(source is null)return Validation("sourceId","The citation source must already be attached to this article group.");
        if(string.IsNullOrWhiteSpace(request.Claim)||request.Claim.Trim().Length>500)return Validation("claim","A claim of at most 500 characters is required.");
        if(request.Locator?.Trim().Length>240)return Validation("locator","The source locator must not exceed 240 characters.");
        var citation=new ArticleClaimCitation(article,source,request.Claim,request.Locator,actor.Id,DateTimeOffset.UtcNow);
        database.ArticleClaimCitations.Add(citation);
        database.AuditLogs.Add(Audit(actor.Id,"editorial.claim_citation_approved",article.Id,new{citation.Id,citation.SourceId,article.LocaleId}));
        await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/articles/{articleId}/claim-citations/{citation.Id}",new{citation.Id,citation.SourceId,sourceName=source.Name,sourceUrl=source.Url,citation.Claim,citation.Locator,citation.ApprovedAt});
    }

    private static async Task<IResult> DeleteClaimCitationAsync(Guid articleId, Guid citationId, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var citation=await database.ArticleClaimCitations.SingleOrDefaultAsync(x=>x.Id==citationId&&x.ArticleLocalizationId==articleId,token);
        var actor=await users.GetUserAsync(principal);if(citation is null||actor is null)return Results.NotFound();
        database.ArticleClaimCitations.Remove(citation);
        database.AuditLogs.Add(Audit(actor.Id,"editorial.claim_citation_removed",articleId,new{citation.Id,citation.SourceId}));
        await database.SaveChangesAsync(token);return Results.NoContent();
    }

    private static async Task<IResult> ListCorrectionsAsync(Guid articleId, PublishingDbContext database, CancellationToken token)
    {
        if (!await database.ArticleLocalizations.AsNoTracking().AnyAsync(x => x.Id == articleId, token)) return Results.NotFound();
        return Results.Ok(await database.ArticleCorrections.AsNoTracking().Where(x => x.ArticleLocalizationId == articleId)
            .OrderByDescending(x => x.CorrectedAt).Select(x => new { x.Id, x.Summary, x.Details, x.ApprovedByUserId, x.CorrectedAt }).ToListAsync(token));
    }

    private static async Task<IResult> CreateCorrectionAsync(Guid articleId, CorrectionRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var article = await database.ArticleLocalizations.SingleOrDefaultAsync(x => x.Id == articleId, token);
        var actor = await users.GetUserAsync(principal);
        if (article is null || actor is null) return Results.NotFound();
        if (article.Status != PublicationStatus.Published) return Results.Conflict(new { message = "Corrections can only be published for a published article." });
        if (string.IsNullOrWhiteSpace(request.Summary) || request.Summary.Trim().Length > 240) return Validation("summary", "A correction summary of at most 240 characters is required.");
        if (string.IsNullOrWhiteSpace(request.Details) || request.Details.Trim().Length > 2000) return Validation("details", "Correction details of at most 2000 characters are required.");
        var correction = new ArticleCorrection(article, request.Summary, request.Details, actor.Id, DateTimeOffset.UtcNow);
        database.ArticleCorrections.Add(correction);
        database.AuditLogs.Add(Audit(actor.Id, "editorial.correction_published", article.Id, new { correction.Id, article.LocaleId, correction.CorrectedAt }));
        await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/articles/{articleId}/corrections/{correction.Id}", new { correction.Id, correction.Summary, correction.Details, correction.CorrectedAt });
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
        var body=ArticleBodyHtmlSanitizer.Sanitize(request.Body);var article = new ArticleLocalization(articleGroup, locale, request.Slug, request.Title, request.Summary ?? string.Empty, body, now);
        article.UpdateDraft(request.Slug, request.Title, request.Summary ?? string.Empty, body, request.SeoTitle, request.SeoDescription, now);
        var categoryIds=request.CategoryIds??[];if(categoryIds.Length==0)return Validation("categoryIds","A category is required.");var categories=await database.Categories.Where(x=>categoryIds.Contains(x.Id)&&x.LocaleId==locale.Id).ToListAsync(token);if(categories.Count!=categoryIds.Distinct().Count())return Validation("categoryIds","A valid category is required.");foreach(var category in categories)article.Categories.Add(category);
        article.UpdateCommercialDisclosure(request.IsSponsored, request.SponsorName, request.HasAffiliateLinks, now);
        await AttachInlineMediaAsync(articleGroup, body, database, token);
        database.ArticleGroups.Add(articleGroup);
        database.AuditLogs.Add(Audit(actor.Id, "editorial.article_created", article.Id, new { request.Locale, type }));
        await database.SaveChangesAsync(token);
        return Results.Created($"/api/v1/admin/articles/{article.Id}", new { article.Id, article.ArticleGroupId, article.UpdatedAt });
    }

    private static async Task<IResult> UpdateAsync(Guid articleId, UpdateArticleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var article = await database.ArticleLocalizations.Include(x => x.Revisions).Include(x => x.Categories).Include(x => x.ArticleGroup).ThenInclude(x => x.MediaAssets).SingleOrDefaultAsync(x => x.Id == articleId, token);
        var actor = await users.GetUserAsync(principal);
        if (article is null || actor is null) return Results.NotFound();
        if (article.UpdatedAt != request.ExpectedUpdatedAt) return Results.Conflict(new { message = "Article changed since it was loaded." });
        if (article.Status != PublicationStatus.Draft) return Results.Conflict(new { message = "Only draft articles can be edited." });
        var now = DateTimeOffset.UtcNow;
        var revision = new ArticleRevision(article, article.Revisions.Count == 0 ? 1 : article.Revisions.Max(x => x.Number) + 1, article.Title, article.Summary, article.Body, actor.Id, now);
        database.ArticleRevisions.Add(revision);
        var categoryIds=request.CategoryIds??[];
        if(categoryIds.Length==0)return Validation("categoryIds","A category is required.");
        var categories=await database.Categories.Where(x=>categoryIds.Contains(x.Id)&&x.LocaleId==article.LocaleId).ToListAsync(token);
        if(categories.Count!=categoryIds.Distinct().Count())return Validation("categoryIds","A valid category is required.");
        var body=ArticleBodyHtmlSanitizer.Sanitize(request.Body);
        article.UpdateDraft(request.Slug, request.Title, request.Summary ?? string.Empty, body, request.SeoTitle, request.SeoDescription, now);
        article.Categories.Clear();foreach(var category in categories)article.Categories.Add(category);
        await AttachInlineMediaAsync(article.ArticleGroup, body, database, token);
        if (article.Status == PublicationStatus.Draft)
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
        PublicationTransitionAsync(articleId, "editorial.published", principal, users, database, x => x.Publish(DateTimeOffset.UtcNow), token);
    private static async Task<IResult> PublishDirectAsync(Guid articleId, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token)
    {
        var article = await database.ArticleLocalizations.SingleOrDefaultAsync(x => x.Id == articleId, token);
        var actor = await users.GetUserAsync(principal);
        if (article is null || actor is null) return Results.NotFound();
        var qualityFailure = await PublicationQualityFailureAsync(articleId, database, token);
        if (qualityFailure is not null) return qualityFailure;
        var now = DateTimeOffset.UtcNow;
        try
        {
            if (article.Status == PublicationStatus.Draft) article.SubmitForEditorialReview(now);
            if (article.Status == PublicationStatus.InEditorialReview) article.ApproveEditorialReview(now);
            if (article.Status is PublicationStatus.InSeoReview or PublicationStatus.Scheduled) article.Publish(now);
            else if (article.Status != PublicationStatus.Published) return Results.Conflict(new { message = "This article cannot be published directly." });
        }
        catch (InvalidOperationException exception) { return Results.Conflict(new { message = exception.Message }); }
        database.AuditLogs.Add(Audit(actor.Id, "editorial.published_directly", article.Id, new { status = article.Status.ToString() }));
        await database.SaveChangesAsync(token);
        return Results.Ok(new { article.Id, status = article.Status.ToString(), article.UpdatedAt, article.PublishedAt });
    }
    private static Task<IResult> ArchiveAsync(Guid articleId, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token) =>
        TransitionAsync(articleId, "editorial.archived", principal, users, database, x => x.Archive(DateTimeOffset.UtcNow), token);

    private static Task<IResult> ScheduleAsync(Guid articleId, ScheduleRequest request, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, CancellationToken token) =>
        PublicationTransitionAsync(articleId, "editorial.scheduled", principal, users, database, x => x.Schedule(request.ScheduledAt, DateTimeOffset.UtcNow), token);

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

    private static async Task<IResult> PublicationTransitionAsync(Guid articleId, string action, System.Security.Claims.ClaimsPrincipal principal, UserManager<ApplicationUser> users, PublishingDbContext database, Action<ArticleLocalization> transition, CancellationToken token)
    {
        var qualityFailure = await PublicationQualityFailureAsync(articleId, database, token);
        return qualityFailure ?? await TransitionAsync(articleId, action, principal, users, database, transition, token);
    }

    private static async Task<IResult?> PublicationQualityFailureAsync(Guid articleId, PublishingDbContext database, CancellationToken token)
    {
        var checklist = await database.ArticleQualityChecklists.AsNoTracking().SingleOrDefaultAsync(x => x.ArticleLocalizationId == articleId, token);
        var missing = PublicationQualityGate.Missing(checklist);
        return missing.Count == 0 ? null : Results.Conflict(new { message = "Publication quality gates are incomplete.", code = "publication_quality_incomplete", missing });
    }

    private static AuditLog Audit(Guid actorId, string action, Guid entityId, object? details) =>
        new(actorId, action, nameof(ArticleLocalization), entityId, details is null ? null : JsonSerializer.Serialize(details), DateTimeOffset.UtcNow);
    private static IResult Validation(string key, string message) => Results.ValidationProblem(new Dictionary<string, string[]> { [key] = [message] });

    private static async Task AttachInlineMediaAsync(ArticleGroup group,string body,PublishingDbContext database,CancellationToken token)
    {
        var ids=InlineMediaPattern().Matches(body).Select(match=>Guid.Parse(match.Groups[1].Value)).Distinct().ToArray();
        if(ids.Length==0)return;
        var assets=await database.MediaAssets.Where(asset=>ids.Contains(asset.Id)).ToListAsync(token);
        if(assets.Count!=ids.Length)throw new InvalidOperationException("One or more inline media assets are invalid.");
        foreach(var asset in assets.Where(asset=>group.MediaAssets.All(existing=>existing.Id!=asset.Id)))group.MediaAssets.Add(asset);
    }
    private sealed record CreateArticleRequest(string Type, string Locale, string Slug, string Title, string? Summary, string? Body, string? SeoTitle, string? SeoDescription, bool IsSponsored, string? SponsorName, bool HasAffiliateLinks, Guid[]? CategoryIds);
    private sealed record UpdateArticleRequest(string Slug, string Title, string? Summary, string? Body, string? SeoTitle, string? SeoDescription, bool IsSponsored, string? SponsorName, bool HasAffiliateLinks, Guid[]? CategoryIds, DateTimeOffset ExpectedUpdatedAt);
    private sealed record ScheduleRequest(DateTimeOffset ScheduledAt);
    private sealed record CorrectionRequest(string Summary, string Details);
    private sealed record ClaimCitationRequest(Guid SourceId, string Claim, string? Locator);
    private sealed record RelationshipsRequest(Guid[] CategoryIds, Guid[] TagIds, Guid[] AuthorIds, Guid[] SourceIds, Guid[] MediaAssetIds, Guid? CoverMediaAssetId, string? CoverAltText, string? CoverCaption, string? CoverCredit);
    [GeneratedRegex(@"/api/media/([0-9a-fA-F-]{36})")]
    private static partial Regex InlineMediaPattern();
}
