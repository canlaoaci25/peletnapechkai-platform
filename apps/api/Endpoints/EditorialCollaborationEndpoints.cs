using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class EditorialCollaborationEndpoints
{
    public static IEndpointRouteBuilder MapEditorialCollaborationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group=endpoints.MapGroup("/api/v1/admin/articles/{articleId:guid}/collaboration").RequireAuthorization(AuthorizationPolicies.WriteContent);
        group.MapGet("/",GetAsync);
        group.MapPost("/tasks",CreateTaskAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        group.MapPost("/tasks/{taskId:guid}/status",SetTaskStatusAsync).ValidateAntiforgery();
        group.MapPost("/comments",CreateCommentAsync).ValidateAntiforgery();
        group.MapPost("/comments/{commentId:guid}/resolve",ResolveCommentAsync).ValidateAntiforgery();
        group.MapPut("/checklist",UpdateChecklistAsync).RequireAuthorization(AuthorizationPolicies.ManageEditorial).ValidateAntiforgery();
        return endpoints;
    }

    private static async Task<IResult> GetAsync(Guid articleId,PublishingDbContext db,CancellationToken token)
    {
        if(!await db.ArticleLocalizations.AnyAsync(x=>x.Id==articleId,token))return Results.NotFound();
        var tasks=await db.EditorialTasks.AsNoTracking().Where(x=>x.ArticleLocalizationId==articleId).OrderBy(x=>x.DueAt).Select(x=>new{x.Id,x.AssigneeUserId,assignee=db.Users.Where(u=>u.Id==x.AssigneeUserId).Select(u=>u.DisplayName).FirstOrDefault(),x.Title,priority=x.Priority.ToString(),status=x.Status.ToString(),x.DueAt}).ToListAsync(token);
        var comments=await db.EditorialComments.AsNoTracking().Where(x=>x.ArticleLocalizationId==articleId).OrderBy(x=>x.CreatedAt).Select(x=>new{x.Id,author=db.Users.Where(u=>u.Id==x.AuthorUserId).Select(u=>u.DisplayName).FirstOrDefault(),x.Body,x.ParentCommentId,x.ArticleRevisionId,x.IsResolved,x.DeletedAt,x.CreatedAt}).ToListAsync(token);
        var checklist=await db.ArticleQualityChecklists.AsNoTracking().Where(x=>x.ArticleLocalizationId==articleId).Select(x=>new{x.TitleAndSummary,x.SourcesVerified,x.AuthorAndTaxonomy,x.SeoMetadata,x.CoverAccessibility,x.CommercialDisclosure,x.TranslationReviewed,x.LegalEditorialReview,isComplete=x.TitleAndSummary&&x.SourcesVerified&&x.AuthorAndTaxonomy&&x.SeoMetadata&&x.CoverAccessibility&&x.CommercialDisclosure&&x.TranslationReviewed&&x.LegalEditorialReview}).SingleOrDefaultAsync(token);
        var users=await db.Users.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.DisplayName).Select(x=>new{x.Id,x.DisplayName}).ToListAsync(token);
        return Results.Ok(new{tasks,comments,checklist,users});
    }

    private static async Task<IResult> CreateTaskAsync(Guid articleId,TaskRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var article=await db.ArticleLocalizations.SingleOrDefaultAsync(x=>x.Id==articleId,token);
        if(actor is null)return Results.Unauthorized();if(article is null||!await db.Users.AnyAsync(x=>x.Id==request.AssigneeUserId&&x.IsActive,token)||!Enum.TryParse<EditorialTaskPriority>(request.Priority,true,out var priority))return Results.BadRequest();
        try{var item=new EditorialTask(article,request.AssigneeUserId,request.Title,priority,request.DueAt,actor.Id,DateTimeOffset.UtcNow);db.Add(item);await db.SaveChangesAsync(token);return Results.Created($"/api/v1/admin/articles/{articleId}/collaboration/tasks/{item.Id}",new{item.Id});}catch(ArgumentException exception){return Results.BadRequest(new{message=exception.Message});}
    }

    private static async Task<IResult> SetTaskStatusAsync(Guid articleId,Guid taskId,StatusRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var item=await db.EditorialTasks.SingleOrDefaultAsync(x=>x.Id==taskId&&x.ArticleLocalizationId==articleId,token);if(actor is null)return Results.Unauthorized();if(item is null)return Results.NotFound();if(item.AssigneeUserId!=actor.Id&&!principal.IsInRole(RoleNames.Owner)&&!principal.IsInRole(RoleNames.Admin)&&!principal.IsInRole(RoleNames.Editor))return Results.Forbid();if(!Enum.TryParse<EditorialTaskStatus>(request.Status,true,out var status))return Results.BadRequest();var previous=item.Status;var now=DateTimeOffset.UtcNow;item.ChangeStatus(status,now);db.AuditLogs.Add(new AuditLog(actor.Id,"editorial.task_status_changed",nameof(EditorialTask),item.Id,System.Text.Json.JsonSerializer.Serialize(new{articleId,previous=previous.ToString(),status=status.ToString()}),now));await db.SaveChangesAsync(token);return Results.Ok();
    }

    private static async Task<IResult> CreateCommentAsync(Guid articleId,CommentRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var actor=await users.GetUserAsync(principal);var article=await db.ArticleLocalizations.SingleOrDefaultAsync(x=>x.Id==articleId,token);if(actor is null)return Results.Unauthorized();if(article is null||string.IsNullOrWhiteSpace(request.Body)||request.Body.Length>4000)return Results.BadRequest();if(request.ParentCommentId is not null&&!await db.EditorialComments.AnyAsync(x=>x.Id==request.ParentCommentId&&x.ArticleLocalizationId==articleId,token))return Results.BadRequest();var item=new EditorialComment(article,request.Body,actor.Id,request.ParentCommentId,request.ArticleRevisionId,DateTimeOffset.UtcNow);db.Add(item);await db.SaveChangesAsync(token);return Results.Created($"/api/v1/admin/articles/{articleId}/collaboration/comments/{item.Id}",new{item.Id});
    }

    private static async Task<IResult> ResolveCommentAsync(Guid articleId,Guid commentId,ResolveRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {var actor=await users.GetUserAsync(principal);var item=await db.EditorialComments.SingleOrDefaultAsync(x=>x.Id==commentId&&x.ArticleLocalizationId==articleId,token);if(actor is null)return Results.Unauthorized();if(item is null)return Results.NotFound();item.Resolve(request.Resolved,DateTimeOffset.UtcNow);await db.SaveChangesAsync(token);return Results.Ok();}

    private static async Task<IResult> UpdateChecklistAsync(Guid articleId,ChecklistRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {var actor=await users.GetUserAsync(principal);var article=await db.ArticleLocalizations.SingleOrDefaultAsync(x=>x.Id==articleId,token);if(actor is null)return Results.Unauthorized();if(article is null)return Results.NotFound();var item=await db.ArticleQualityChecklists.SingleOrDefaultAsync(x=>x.ArticleLocalizationId==articleId,token)??new ArticleQualityChecklist(article);item.Update(request.TitleAndSummary,request.SourcesVerified,request.AuthorAndTaxonomy,request.SeoMetadata,request.CoverAccessibility,request.CommercialDisclosure,request.TranslationReviewed,request.LegalEditorialReview,actor.Id,DateTimeOffset.UtcNow);if(db.Entry(item).State==EntityState.Detached)db.Add(item);await db.SaveChangesAsync(token);return Results.Ok(new{item.IsComplete});}

    private sealed record TaskRequest(Guid AssigneeUserId,string Title,string Priority,DateTimeOffset DueAt);private sealed record StatusRequest(string Status);private sealed record CommentRequest(string Body,Guid?ParentCommentId,Guid?ArticleRevisionId);private sealed record ResolveRequest(bool Resolved);private sealed record ChecklistRequest(bool TitleAndSummary,bool SourcesVerified,bool AuthorAndTaxonomy,bool SeoMetadata,bool CoverAccessibility,bool CommercialDisclosure,bool TranslationReviewed,bool LegalEditorialReview);
}
