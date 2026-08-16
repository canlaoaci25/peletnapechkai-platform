using Microsoft.AspNetCore.Identity;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;

namespace Peletnapechkai.Api.Endpoints;

public static class MemberAccountEndpoints
{
    public static IEndpointRouteBuilder MapMemberAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group=endpoints.MapGroup("/api/v1/account").RequireAuthorization().WithTags("Member account");
        group.MapGet("/",GetAsync);
        group.MapPut("/profile",UpdateProfileAsync).ValidateAntiforgery();
        group.MapPost("/password",ChangePasswordAsync).ValidateAntiforgery();
        group.MapPost("/email-verification",EmailVerificationStatus).ValidateAntiforgery();
        group.MapGet("/saved", ListSavedAsync);
        group.MapGet("/saved/{locale}/{slug}", GetSavedStatusAsync);
        group.MapPut("/saved/{locale}/{slug}", SaveArticleAsync).ValidateAntiforgery();
        group.MapDelete("/saved/{locale}/{slug}", RemoveSavedArticleAsync).ValidateAntiforgery();
        group.MapGet("/following", ListFollowingAsync);
        group.MapGet("/following/{locale}/{slug}", GetFollowingStatusAsync);
        group.MapPut("/following/{locale}/{slug}", FollowCategoryAsync).ValidateAntiforgery();
        group.MapDelete("/following/{locale}/{slug}", UnfollowCategoryAsync).ValidateAntiforgery();
        group.MapGet("/feed", GetPersonalFeedAsync);
        return endpoints;
    }
    private static async Task<IResult> GetAsync(System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,IConfiguration configuration)
    {var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();var roles=await users.GetRolesAsync(user);return Results.Ok(new{user.Id,user.Email,user.DisplayName,user.EmailConfirmed,roles,verificationAvailable=!string.IsNullOrWhiteSpace(configuration["Email:SmtpHost"]),user.CreatedAt});}
    private static async Task<IResult> UpdateProfileAsync(ProfileRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,SignInManager<ApplicationUser> signIn,PublishingDbContext db,CancellationToken token)
    {var user=await users.GetUserAsync(principal);var name=request.DisplayName?.Trim();if(user is null)return Results.Unauthorized();if(string.IsNullOrWhiteSpace(name)||name.Length is <2 or >160)return Results.BadRequest(new{message="Display name must be between 2 and 160 characters."});user.DisplayName=name;var result=await users.UpdateAsync(user);if(!result.Succeeded)return Results.BadRequest(new{message="Profile could not be updated."});await signIn.RefreshSignInAsync(user);db.AuditLogs.Add(new AuditLog(user.Id,"identity.member_profile_updated",nameof(ApplicationUser),user.Id,null,DateTimeOffset.UtcNow));await db.SaveChangesAsync(token);return Results.NoContent();}
    private static async Task<IResult> ChangePasswordAsync(PasswordRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,SignInManager<ApplicationUser> signIn,PublishingDbContext db,CancellationToken token)
    {var user=await users.GetUserAsync(principal);if(user is null)return Results.Unauthorized();var result=await users.ChangePasswordAsync(user,request.CurrentPassword??"",request.NewPassword??"");if(!result.Succeeded)return Results.ValidationProblem(result.Errors.GroupBy(x=>x.Code).ToDictionary(x=>x.Key,x=>x.Select(y=>y.Description).ToArray()));await signIn.RefreshSignInAsync(user);db.AuditLogs.Add(new AuditLog(user.Id,"identity.member_password_changed",nameof(ApplicationUser),user.Id,null,DateTimeOffset.UtcNow));await db.SaveChangesAsync(token);return Results.NoContent();}
    private static async Task<IResult> EmailVerificationStatus(System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,IConfiguration configuration)
    {var user=await users.GetUserAsync(principal);if(user is null)return Results.Unauthorized();if(user.EmailConfirmed)return Results.Ok(new{confirmed=true,message="Email is already verified."});if(string.IsNullOrWhiteSpace(configuration["Email:SmtpHost"]))return Results.Json(new{confirmed=false,configured=false,message="Email provider is not configured."},statusCode:503);return Results.Accepted(value:new{confirmed=false,configured=true,message="Verification delivery is configured; sender integration is pending activation."});}
    private static async Task<IResult> ListSavedAsync(string? locale,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var query=db.SavedArticles.AsNoTracking().Where(item=>item.UserId==user.Id&&item.ArticleLocalization.Status==PublicationStatus.Published);
        if(!string.IsNullOrWhiteSpace(locale))query=query.Where(item=>item.ArticleLocalization.Locale.Code==locale);
        var items=await query.OrderByDescending(item=>item.SavedAt).Select(item=>new{item.ArticleLocalization.Slug,item.ArticleLocalization.Title,item.ArticleLocalization.Summary,type=item.ArticleLocalization.ArticleGroup.Type.ToString(),locale=item.ArticleLocalization.Locale.Code,item.ArticleLocalization.PublishedAt,item.SavedAt,cover=item.ArticleLocalization.CoverMediaAssetId==null?null:new{url="/api/media/"+item.ArticleLocalization.CoverMediaAssetId+"?v="+item.ArticleLocalization.CoverMediaAsset!.OptimizedByteLength,altText=item.ArticleLocalization.CoverAltText}}).ToArrayAsync(token);
        return Results.Ok(items);
    }
    private static async Task<IResult> GetSavedStatusAsync(string locale,string slug,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var saved=await db.SavedArticles.AsNoTracking().AnyAsync(item=>item.UserId==user.Id&&item.ArticleLocalization.Locale.Code==locale&&item.ArticleLocalization.Slug==slug&&item.ArticleLocalization.Status==PublicationStatus.Published,token);
        return Results.Ok(new{saved});
    }
    private static async Task<IResult> SaveArticleAsync(string locale,string slug,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var article=await db.ArticleLocalizations.SingleOrDefaultAsync(item=>item.Locale.Code==locale&&item.Slug==slug&&item.Status==PublicationStatus.Published,token);if(article is null)return Results.NotFound();
        if(await db.SavedArticles.AnyAsync(item=>item.UserId==user.Id&&item.ArticleLocalizationId==article.Id,token))return Results.NoContent();
        db.SavedArticles.Add(new SavedArticle(user,article,DateTimeOffset.UtcNow));
        db.AuditLogs.Add(new AuditLog(user.Id,"member.article_saved",nameof(ArticleLocalization),article.Id,null,DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(token);return Results.NoContent();
    }
    private static async Task<IResult> RemoveSavedArticleAsync(string locale,string slug,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var saved=await db.SavedArticles.Include(item=>item.ArticleLocalization).SingleOrDefaultAsync(item=>item.UserId==user.Id&&item.ArticleLocalization.Locale.Code==locale&&item.ArticleLocalization.Slug==slug,token);if(saved is null)return Results.NoContent();
        db.SavedArticles.Remove(saved);db.AuditLogs.Add(new AuditLog(user.Id,"member.article_unsaved",nameof(ArticleLocalization),saved.ArticleLocalizationId,null,DateTimeOffset.UtcNow));await db.SaveChangesAsync(token);return Results.NoContent();
    }
    private static async Task<IResult> ListFollowingAsync(string? locale,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var query=db.FollowedCategories.AsNoTracking().Where(item=>item.UserId==user.Id);
        if(!string.IsNullOrWhiteSpace(locale))query=query.Where(item=>item.Category.Locale.Code==locale);
        return Results.Ok(await query.OrderByDescending(item=>item.FollowedAt).Select(item=>new{item.Category.Slug,title=item.Category.Name,description=item.Category.Description,locale=item.Category.Locale.Code,item.FollowedAt,articleCount=item.Category.Articles.Count(article=>article.Status==PublicationStatus.Published)}).ToArrayAsync(token));
    }
    private static async Task<IResult> GetFollowingStatusAsync(string locale,string slug,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        return Results.Ok(new{following=await db.FollowedCategories.AsNoTracking().AnyAsync(item=>item.UserId==user.Id&&item.Category.Locale.Code==locale&&item.Category.Slug==slug,token)});
    }
    private static async Task<IResult> FollowCategoryAsync(string locale,string slug,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var category=await db.Categories.SingleOrDefaultAsync(item=>item.Locale.Code==locale&&item.Slug==slug,token);if(category is null)return Results.NotFound();
        if(await db.FollowedCategories.AnyAsync(item=>item.UserId==user.Id&&item.CategoryId==category.Id,token))return Results.NoContent();
        db.FollowedCategories.Add(new FollowedCategory(user,category,DateTimeOffset.UtcNow));
        db.AuditLogs.Add(new AuditLog(user.Id,"member.category_followed",nameof(Category),category.Id,null,DateTimeOffset.UtcNow));
        await db.SaveChangesAsync(token);return Results.NoContent();
    }
    private static async Task<IResult> UnfollowCategoryAsync(string locale,string slug,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var followed=await db.FollowedCategories.Include(item=>item.Category).SingleOrDefaultAsync(item=>item.UserId==user.Id&&item.Category.Locale.Code==locale&&item.Category.Slug==slug,token);if(followed is null)return Results.NoContent();
        db.FollowedCategories.Remove(followed);db.AuditLogs.Add(new AuditLog(user.Id,"member.category_unfollowed",nameof(Category),followed.CategoryId,null,DateTimeOffset.UtcNow));await db.SaveChangesAsync(token);return Results.NoContent();
    }
    private static async Task<IResult> GetPersonalFeedAsync(string locale,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var items=await db.ArticleLocalizations.AsNoTracking().Where(article=>article.Locale.Code==locale&&article.Status==PublicationStatus.Published&&article.Categories.Any(category=>db.FollowedCategories.Any(follow=>follow.UserId==user.Id&&follow.CategoryId==category.Id))).OrderByDescending(article=>article.PublishedAt).Take(12).Select(article=>new{article.Slug,article.Title,article.Summary,type=article.ArticleGroup.Type.ToString(),locale=article.Locale.Code,article.PublishedAt,categories=article.Categories.Where(category=>db.FollowedCategories.Any(follow=>follow.UserId==user.Id&&follow.CategoryId==category.Id)).Select(category=>category.Name).ToArray(),cover=article.CoverMediaAssetId==null?null:new{url="/api/media/"+article.CoverMediaAssetId+"?v="+article.CoverMediaAsset!.OptimizedByteLength,altText=article.CoverAltText}}).ToArrayAsync(token);
        return Results.Ok(items);
    }
    public sealed record ProfileRequest(string? DisplayName);
    public sealed record PasswordRequest(string? CurrentPassword,string? NewPassword);
}
