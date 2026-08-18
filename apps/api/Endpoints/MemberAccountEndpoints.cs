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
        group.MapPut("/following-setup/{locale}", SetupFollowingAsync).ValidateAntiforgery();
        group.MapGet("/feed", GetPersonalFeedAsync);
        group.MapGet("/reading-progress", ListReadingProgressAsync);
        group.MapGet("/reading-progress/{locale}/{slug}", GetReadingProgressAsync);
        group.MapPut("/reading-progress/{locale}/{slug}", UpdateReadingProgressAsync).ValidateAntiforgery();
        group.MapGet("/reading-ritual", GetReadingRitualAsync);
        group.MapPut("/reading-ritual", UpdateReadingRitualAsync).ValidateAntiforgery();
        group.MapGet("/reading-digest", GetReadingDigestAsync);
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
    private static async Task<IResult> SetupFollowingAsync(string locale,FollowingSetupRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var slugs=(request.Slugs??[]).Select(value=>value?.Trim()).Where(value=>!string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if(slugs.Length is <1 or >5)return Results.BadRequest(new{message="Choose between 1 and 5 topics."});
        var categories=await db.Categories.Where(item=>item.Locale.Code==locale&&slugs.Contains(item.Slug)&&item.ParentCategoryId==null&&item.Articles.Any(article=>article.Status==PublicationStatus.Published)).ToArrayAsync(token);
        if(categories.Length!=slugs.Length)return Results.BadRequest(new{message="One or more topics are unavailable."});
        var existing=await db.FollowedCategories.Where(item=>item.UserId==user.Id&&item.Category.Locale.Code==locale).Select(item=>item.CategoryId).ToArrayAsync(token);var now=DateTimeOffset.UtcNow;
        foreach(var category in categories.Where(item=>!existing.Contains(item.Id)))db.FollowedCategories.Add(new FollowedCategory(user,category,now));
        db.AuditLogs.Add(new AuditLog(user.Id,"member.topic_onboarding_completed",nameof(ApplicationUser),user.Id,System.Text.Json.JsonSerializer.Serialize(new{locale,topicCount=categories.Length}),now));
        await db.SaveChangesAsync(token);return Results.Ok(new{followed=categories.Length});
    }
    private static async Task<IResult> GetPersonalFeedAsync(string locale,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var items=await db.ArticleLocalizations.AsNoTracking().Where(article=>article.Locale.Code==locale&&article.Status==PublicationStatus.Published&&article.Categories.Any(category=>db.FollowedCategories.Any(follow=>follow.UserId==user.Id&&follow.CategoryId==category.Id))).OrderByDescending(article=>article.PublishedAt).Take(12).Select(article=>new{article.Slug,article.Title,article.Summary,type=article.ArticleGroup.Type.ToString(),locale=article.Locale.Code,article.PublishedAt,categories=article.Categories.Where(category=>db.FollowedCategories.Any(follow=>follow.UserId==user.Id&&follow.CategoryId==category.Id)).Select(category=>category.Name).ToArray(),cover=article.CoverMediaAssetId==null?null:new{url="/api/media/"+article.CoverMediaAssetId+"?v="+article.CoverMediaAsset!.OptimizedByteLength,altText=article.CoverAltText}}).ToArrayAsync(token);
        return Results.Ok(items);
    }
    private static async Task<IResult> ListReadingProgressAsync(string locale,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var items=await db.ArticleReadingProgress.AsNoTracking().Where(item=>item.UserId==user.Id&&item.Percent>=5&&item.Percent<95&&item.ArticleLocalization.Locale.Code==locale&&item.ArticleLocalization.Status==PublicationStatus.Published).OrderByDescending(item=>item.LastReadAt).Take(8).Select(item=>new{item.ArticleLocalization.Slug,item.ArticleLocalization.Title,item.ArticleLocalization.Summary,locale=item.ArticleLocalization.Locale.Code,item.Percent,item.Anchor,item.LastReadAt,cover=item.ArticleLocalization.CoverMediaAssetId==null?null:new{url="/api/media/"+item.ArticleLocalization.CoverMediaAssetId+"?v="+item.ArticleLocalization.CoverMediaAsset!.OptimizedByteLength,altText=item.ArticleLocalization.CoverAltText}}).ToArrayAsync(token);
        return Results.Ok(items);
    }
    private static async Task<IResult> GetReadingProgressAsync(string locale,string slug,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var progress=await db.ArticleReadingProgress.AsNoTracking().Where(item=>item.UserId==user.Id&&item.ArticleLocalization.Locale.Code==locale&&item.ArticleLocalization.Slug==slug&&item.ArticleLocalization.Status==PublicationStatus.Published).Select(item=>new{item.Percent,item.Anchor,item.LastReadAt}).SingleOrDefaultAsync(token);
        return progress is null?Results.Ok(new{percent=0,anchor=(string?)null,lastReadAt=(DateTimeOffset?)null}):Results.Ok(progress);
    }
    private static async Task<IResult> UpdateReadingProgressAsync(string locale,string slug,ReadingProgressRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        if(request.Percent is <0 or >100||request.Anchor?.Length>160)return Results.BadRequest();
        var article=await db.ArticleLocalizations.SingleOrDefaultAsync(item=>item.Locale.Code==locale&&item.Slug==slug&&item.Status==PublicationStatus.Published,token);if(article is null)return Results.NotFound();
        var progress=await db.ArticleReadingProgress.SingleOrDefaultAsync(item=>item.UserId==user.Id&&item.ArticleLocalizationId==article.Id,token);var now=DateTimeOffset.UtcNow;
        if(progress is null){db.ArticleReadingProgress.Add(new ArticleReadingProgress(user,article,request.Percent,request.Anchor,now));db.AuditLogs.Add(new AuditLog(user.Id,"member.article_reading_started",nameof(ArticleLocalization),article.Id,null,now));}else progress.Update(request.Percent,request.Anchor,now);
        await db.SaveChangesAsync(token);return Results.NoContent();
    }
    private static async Task<IResult> GetReadingRitualAsync(string locale,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var now=DateTimeOffset.UtcNow;var daysFromMonday=((int)now.DayOfWeek+6)%7;var weekStart=new DateTimeOffset(now.UtcDateTime.Date,TimeSpan.Zero).AddDays(-daysFromMonday);
        var completed=await db.ArticleReadingProgress.AsNoTracking().Where(item=>item.UserId==user.Id&&item.CompletedAt>=weekStart&&item.ArticleLocalization.Locale.Code==locale&&item.ArticleLocalization.Status==PublicationStatus.Published).Select(item=>item.CompletedAt!.Value).ToArrayAsync(token);
        var next=await db.ArticleLocalizations.AsNoTracking().Where(article=>article.Locale.Code==locale&&article.Status==PublicationStatus.Published&&!db.ArticleReadingProgress.Any(progress=>progress.UserId==user.Id&&progress.ArticleLocalizationId==article.Id&&progress.CompletedAt!=null)&&(article.Categories.Any(category=>db.FollowedCategories.Any(follow=>follow.UserId==user.Id&&follow.CategoryId==category.Id))||db.SavedArticles.Any(saved=>saved.UserId==user.Id&&saved.ArticleLocalizationId==article.Id))).OrderByDescending(article=>article.PublishedAt).Select(article=>new{article.Slug,article.Title,article.Summary,cover=article.CoverMediaAssetId==null?null:new{url="/api/media/"+article.CoverMediaAssetId+"?v="+article.CoverMediaAsset!.OptimizedByteLength,altText=article.CoverAltText}}).FirstOrDefaultAsync(token);
        return Results.Ok(new{goal=user.WeeklyReadingGoal,completed=completed.Length,activeDays=completed.Select(item=>item.UtcDateTime.Date).Distinct().Count(),weekStartsAt=weekStart,next});
    }
    private static async Task<IResult> UpdateReadingRitualAsync(ReadingRitualRequest request,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();if(request.Goal is not (1 or 3 or 5))return Results.BadRequest(new{message="Weekly reading goal must be 1, 3, or 5."});
        if(user.WeeklyReadingGoal==request.Goal)return Results.NoContent();user.WeeklyReadingGoal=request.Goal;db.AuditLogs.Add(new AuditLog(user.Id,"member.weekly_reading_goal_updated",nameof(ApplicationUser),user.Id,null,DateTimeOffset.UtcNow));await db.SaveChangesAsync(token);return Results.NoContent();
    }
    private static async Task<IResult> GetReadingDigestAsync(string locale,System.Security.Claims.ClaimsPrincipal principal,UserManager<ApplicationUser> users,PublishingDbContext db,CancellationToken token)
    {
        var user=await users.GetUserAsync(principal);if(user is null||!user.IsActive)return Results.Unauthorized();
        var now=DateTimeOffset.UtcNow;var daysFromMonday=((int)now.DayOfWeek+6)%7;var weekStart=new DateTimeOffset(now.UtcDateTime.Date,TimeSpan.Zero).AddDays(-daysFromMonday);
        var items=await db.ArticleLocalizations.AsNoTracking()
            .Where(article=>article.Locale.Code==locale&&article.Status==PublicationStatus.Published
                &&!db.ArticleReadingProgress.Any(progress=>progress.UserId==user.Id&&progress.ArticleLocalizationId==article.Id&&progress.CompletedAt!=null)
                &&(db.ArticleReadingProgress.Any(progress=>progress.UserId==user.Id&&progress.ArticleLocalizationId==article.Id&&progress.Percent>=5&&progress.Percent<95)
                    ||article.Categories.Any(category=>db.FollowedCategories.Any(follow=>follow.UserId==user.Id&&follow.CategoryId==category.Id))
                    ||db.SavedArticles.Any(saved=>saved.UserId==user.Id&&saved.ArticleLocalizationId==article.Id)))
            .Select(article=>new{
                article.Slug,article.Title,article.Summary,article.PublishedAt,
                reason=db.ArticleReadingProgress.Any(progress=>progress.UserId==user.Id&&progress.ArticleLocalizationId==article.Id&&progress.Percent>=5&&progress.Percent<95)?"continue":article.Categories.Any(category=>db.FollowedCategories.Any(follow=>follow.UserId==user.Id&&follow.CategoryId==category.Id))?"followed":"saved",
                progress=db.ArticleReadingProgress.Where(progress=>progress.UserId==user.Id&&progress.ArticleLocalizationId==article.Id).Select(progress=>(int?)progress.Percent).FirstOrDefault(),
                anchor=db.ArticleReadingProgress.Where(progress=>progress.UserId==user.Id&&progress.ArticleLocalizationId==article.Id).Select(progress=>progress.Anchor).FirstOrDefault(),
                topic=article.Categories.Where(category=>db.FollowedCategories.Any(follow=>follow.UserId==user.Id&&follow.CategoryId==category.Id)).Select(category=>category.Name).FirstOrDefault(),
                cover=article.CoverMediaAssetId==null?null:new{url="/api/media/"+article.CoverMediaAssetId+"?v="+article.CoverMediaAsset!.OptimizedByteLength,altText=article.CoverAltText}
            })
            .OrderBy(item=>item.reason=="continue"?0:item.reason=="followed"?1:2).ThenByDescending(item=>item.PublishedAt).Take(6).ToArrayAsync(token);
        return Results.Ok(new{weekStartsAt=weekStart,generatedAt=now,items});
    }
    public sealed record ProfileRequest(string? DisplayName);
    public sealed record PasswordRequest(string? CurrentPassword,string? NewPassword);
    public sealed record ReadingProgressRequest(int Percent,string? Anchor);
    public sealed record ReadingRitualRequest(int Goal);
    public sealed record FollowingSetupRequest(string[]? Slugs);
}
