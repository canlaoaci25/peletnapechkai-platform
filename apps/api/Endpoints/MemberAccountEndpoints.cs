using Microsoft.AspNetCore.Identity;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

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
    public sealed record ProfileRequest(string? DisplayName);
    public sealed record PasswordRequest(string? CurrentPassword,string? NewPassword);
}
