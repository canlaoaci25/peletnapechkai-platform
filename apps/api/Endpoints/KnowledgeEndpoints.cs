using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Domain.Knowledge;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class KnowledgeEndpoints
{
    public static IEndpointRouteBuilder MapKnowledgeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group=endpoints.MapGroup("/api/v1/admin/knowledge").RequireAuthorization(AuthorizationPolicies.ManageEditorial).WithTags("Knowledge Vault");
        group.MapGet("/",List);
        group.MapPost("/",Create).ValidateAntiforgery();
        group.MapPost("/{id:guid}/approve",(Guid id,System.Security.Claims.ClaimsPrincipal p,UserManager<ApplicationUser>u,PublishingDbContext d,CancellationToken t)=>Review(id,true,p,u,d,t)).ValidateAntiforgery();
        group.MapPost("/{id:guid}/reject",(Guid id,System.Security.Claims.ClaimsPrincipal p,UserManager<ApplicationUser>u,PublishingDbContext d,CancellationToken t)=>Review(id,false,p,u,d,t)).ValidateAntiforgery();
        group.MapPost("/{id:guid}/links",LinkArticle).ValidateAntiforgery();
        group.MapPost("/links/{linkId:guid}/verify",VerifyLink).ValidateAntiforgery();
        group.MapPost("/links/{linkId:guid}/remove",RemoveLink).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> List(PublishingDbContext db,CancellationToken token)=>Results.Ok(await db.KnowledgeCandidates.AsNoTracking().OrderByDescending(x=>x.UpdatedAt).Take(200).Select(x=>new{x.Id,locale=x.Locale.Code,x.Title,x.Claim,x.SourceUrl,x.AiAssisted,status=x.Status.ToString(),x.CreatedAt,x.UpdatedAt,links=db.KnowledgeArticleLinks.Where(link=>link.KnowledgeCandidateId==x.Id).OrderBy(link=>link.ReviewDueAt).Select(link=>new{link.Id,link.ArticleLocalizationId,articleTitle=link.Article.Title,articleSlug=link.Article.Slug,purpose=link.Purpose.ToString(),link.Note,link.ReviewDueAt,link.LastVerifiedAt})}).ToListAsync(token));

    private static async Task<IResult> Create(Request request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser>users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var locale=await db.Locales.SingleOrDefaultAsync(x=>x.Code==request.Locale&&x.IsEnabled,token);
        if(actor is null)return Results.Unauthorized();
        if(locale is null||string.IsNullOrWhiteSpace(request.Title)||request.Title.Length>240||string.IsNullOrWhiteSpace(request.Claim)||request.Claim.Length>4000||!Uri.TryCreate(request.SourceUrl,UriKind.Absolute,out var url)||url.Scheme is not("http" or "https"))return Results.BadRequest();
        var item=new KnowledgeCandidate(locale,request.Title,request.Claim,url,request.AiAssisted,actor.Id,DateTimeOffset.UtcNow);db.Add(item);db.AuditLogs.Add(Audit(actor.Id,"knowledge.candidate_created",nameof(KnowledgeCandidate),item.Id,new{request.AiAssisted}));await db.SaveChangesAsync(token);return Results.Created($"/api/v1/admin/knowledge/{item.Id}",new{item.Id});
    }

    private static async Task<IResult> Review(Guid id,bool approve,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser>users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var item=await db.KnowledgeCandidates.SingleOrDefaultAsync(x=>x.Id==id,token);if(actor is null)return Results.Unauthorized();if(item is null)return Results.NotFound();
        try{item.Review(approve,actor.Id,DateTimeOffset.UtcNow);}catch(InvalidOperationException exception){return Results.Conflict(new{message=exception.Message});}
        db.AuditLogs.Add(Audit(actor.Id,approve?"knowledge.approved":"knowledge.rejected",nameof(KnowledgeCandidate),item.Id,null));await db.SaveChangesAsync(token);return Results.Ok(new{item.Id,status=item.Status.ToString()});
    }

    private static async Task<IResult> LinkArticle(Guid id,LinkRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser>users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);if(actor is null)return Results.Unauthorized();
        var candidate=await db.KnowledgeCandidates.SingleOrDefaultAsync(x=>x.Id==id,token);var article=await db.ArticleLocalizations.SingleOrDefaultAsync(x=>x.Id==request.ArticleLocalizationId,token);
        if(candidate is null||article is null)return Results.NotFound();if(!Enum.TryParse<KnowledgeUsePurpose>(request.Purpose,true,out var purpose))return Results.BadRequest();
        try{var link=new KnowledgeArticleLink(candidate,article,purpose,request.Note,request.ReviewDueAt,actor.Id,DateTimeOffset.UtcNow);db.KnowledgeArticleLinks.Add(link);db.AuditLogs.Add(Audit(actor.Id,"knowledge.article_linked",nameof(KnowledgeArticleLink),link.Id,new{candidateId=id,articleId=article.Id,purpose}));await db.SaveChangesAsync(token);return Results.Created($"/api/v1/admin/knowledge/{id}/links/{link.Id}",new{link.Id});}
        catch(InvalidOperationException exception){return Results.Conflict(new{message=exception.Message});}catch(ArgumentOutOfRangeException exception){return Results.ValidationProblem(new Dictionary<string,string[]>{{"reviewDueAt",[exception.Message]}});}catch(DbUpdateException){return Results.Conflict(new{message="This knowledge candidate is already linked to the article."});}
    }

    private static async Task<IResult> VerifyLink(Guid linkId,VerifyRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser>users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var link=await db.KnowledgeArticleLinks.SingleOrDefaultAsync(x=>x.Id==linkId,token);if(actor is null)return Results.Unauthorized();if(link is null)return Results.NotFound();
        try{link.Verify(request.NextReviewDueAt,actor.Id,DateTimeOffset.UtcNow);}catch(ArgumentOutOfRangeException exception){return Results.ValidationProblem(new Dictionary<string,string[]>{{"nextReviewDueAt",[exception.Message]}});}
        db.AuditLogs.Add(Audit(actor.Id,"knowledge.link_verified",nameof(KnowledgeArticleLink),link.Id,new{request.NextReviewDueAt}));await db.SaveChangesAsync(token);return Results.Ok(new{link.Id,link.LastVerifiedAt,link.ReviewDueAt});
    }

    private static async Task<IResult> RemoveLink(Guid linkId,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser>users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var link=await db.KnowledgeArticleLinks.SingleOrDefaultAsync(x=>x.Id==linkId,token);if(actor is null)return Results.Unauthorized();if(link is null)return Results.NotFound();
        db.AuditLogs.Add(Audit(actor.Id,"knowledge.article_unlinked",nameof(KnowledgeArticleLink),link.Id,new{link.KnowledgeCandidateId,link.ArticleLocalizationId}));db.KnowledgeArticleLinks.Remove(link);await db.SaveChangesAsync(token);return Results.NoContent();
    }

    private static AuditLog Audit(Guid actor,string action,string entityType,Guid id,object?details)=>new(actor,action,entityType,id,details is null?null:JsonSerializer.Serialize(details),DateTimeOffset.UtcNow);
    private sealed record Request(string Locale,string Title,string Claim,string SourceUrl,bool AiAssisted);
    private sealed record LinkRequest(Guid ArticleLocalizationId,string Purpose,string? Note,DateTimeOffset ReviewDueAt);
    private sealed record VerifyRequest(DateTimeOffset NextReviewDueAt);
}
