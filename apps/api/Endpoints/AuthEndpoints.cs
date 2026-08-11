using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Authentication");

        group.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken });
        }).AllowAnonymous();

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(IdentityServiceExtensions.LoginRateLimitPolicy)
            .ValidateAntiforgery();
        group.MapPost("/register", RegisterAsync).AllowAnonymous().RequireRateLimiting(IdentityServiceExtensions.LoginRateLimitPolicy).ValidateAntiforgery();

        group.MapPost("/login/2fa", TwoFactorLoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(IdentityServiceExtensions.LoginRateLimitPolicy)
            .ValidateAntiforgery();

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .ValidateAntiforgery();

        group.MapGet("/session", SessionAsync).RequireAuthorization();

        group.MapPost("/2fa/setup", SetupTwoFactorAsync)
            .RequireAuthorization()
            .ValidateAntiforgery();
        group.MapPost("/2fa/enable", EnableTwoFactorAsync)
            .RequireAuthorization()
            .ValidateAntiforgery();
        group.MapPost("/2fa/disable", DisableTwoFactorAsync)
            .RequireAuthorization()
            .ValidateAntiforgery();
        group.MapPost("/2fa/recovery-codes", RegenerateRecoveryCodesAsync)
            .RequireAuthorization()
            .ValidateAntiforgery();
        group.MapPost("/session/revoke-all", RevokeAllSessionsAsync)
            .RequireAuthorization()
            .ValidateAntiforgery();

        return endpoints;
    }

    private static async Task<IResult> TwoFactorLoginAsync(
        TwoFactorLoginRequest request,
        SignInManager<ApplicationUser> signInManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        var pendingUser = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (pendingUser is null)
        {
            return Results.Unauthorized();
        }

        SignInResult result;
        if (!string.IsNullOrWhiteSpace(request.AuthenticatorCode))
        {
            result = await signInManager.TwoFactorAuthenticatorSignInAsync(
                NormalizeAuthenticatorCode(request.AuthenticatorCode), false, false);
        }
        else if (!string.IsNullOrWhiteSpace(request.RecoveryCode))
        {
            result = await signInManager.TwoFactorRecoveryCodeSignInAsync(request.RecoveryCode.Trim());
        }
        else
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["code"] = ["An authenticator code or recovery code is required."]
            });
        }

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        database.AuditLogs.Add(new AuditLog(pendingUser.Id, "identity.login_2fa", nameof(ApplicationUser), pendingUser.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["credentials"] = ["Email and password are required."]
            });
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !user.IsActive)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, true);
        if (result.RequiresTwoFactor)
        {
            return Results.Json(new { twoFactorRequired = true }, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        database.AuditLogs.Add(new AuditLog(user.Id, "identity.login", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RegisterAsync(RegisterRequest request,UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager,PublishingDbContext database,CancellationToken cancellationToken)
    {
        var email=request.Email?.Trim();var name=request.DisplayName?.Trim();
        if(string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(name)||name.Length is <2 or >160||string.IsNullOrWhiteSpace(request.Password))return Results.ValidationProblem(new Dictionary<string,string[]>{{"account",["Ad, email and password are required."]}});
        if(await userManager.FindByEmailAsync(email) is not null)return Results.Conflict(new{message="An account with this email already exists."});
        var user=new ApplicationUser{Id=Guid.CreateVersion7(),UserName=email,Email=email,DisplayName=name,EmailConfirmed=true,IsActive=true,CreatedAt=DateTimeOffset.UtcNow};
        var created=await userManager.CreateAsync(user,request.Password);if(!created.Succeeded)return Results.ValidationProblem(created.Errors.GroupBy(x=>x.Code).ToDictionary(x=>x.Key,x=>x.Select(y=>y.Description).ToArray()));
        var role=await userManager.AddToRoleAsync(user,RoleNames.Member);if(!role.Succeeded){await userManager.DeleteAsync(user);throw new InvalidOperationException("Member role assignment failed.");}
        await signInManager.SignInAsync(user,false);database.AuditLogs.Add(new AuditLog(user.Id,"identity.member_registered",nameof(ApplicationUser),user.Id,null,DateTimeOffset.UtcNow));await database.SaveChangesAsync(cancellationToken);return Results.Ok(new{user.Id,user.Email,user.DisplayName});
    }

    private static async Task<IResult> LogoutAsync(
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(principal);
        await signInManager.SignOutAsync();
        if (Guid.TryParse(userId, out var parsedUserId))
        {
            database.AuditLogs.Add(new AuditLog(parsedUserId, "identity.logout", nameof(ApplicationUser), parsedUserId, null, DateTimeOffset.UtcNow));
            await database.SaveChangesAsync(cancellationToken);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> SessionAsync(
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null || !user.IsActive)
        {
            return Results.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        return Results.Ok(new { user.Id, user.Email, user.DisplayName, roles });
    }

    private static async Task<IResult> SetupTwoFactorAsync(
        CurrentPasswordRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        HttpContext context)
    {
        var user = await GetUserWithValidPasswordAsync(principal, request.CurrentPassword, userManager);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            EnsureSucceeded(await userManager.ResetAuthenticatorKeyAsync(user), "Authenticator key reset failed");
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(user.Email))
        {
            throw new InvalidOperationException("Authenticator setup could not be initialized.");
        }

        context.Response.Headers.CacheControl = "no-store";
        var uri = $"otpauth://totp/{Uri.EscapeDataString("Peletnapechkai")}:{Uri.EscapeDataString(user.Email)}?secret={key}&issuer={Uri.EscapeDataString("Peletnapechkai")}&digits=6";
        return Results.Ok(new { sharedKey = key, authenticatorUri = uri });
    }

    private static async Task<IResult> EnableTwoFactorAsync(
        TwoFactorCodeRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        PublishingDbContext database,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(request.Code))
        {
            return Results.Unauthorized();
        }

        var valid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultAuthenticatorProvider,
            NormalizeAuthenticatorCode(request.Code));
        if (!valid)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["code"] = ["The authenticator code is invalid."] });
        }

        EnsureSucceeded(await userManager.SetTwoFactorEnabledAsync(user, true), "Two-factor enable failed");
        var recoveryCodes = (await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))?.ToArray() ?? [];
        await signInManager.RefreshSignInAsync(user);
        database.AuditLogs.Add(new AuditLog(user.Id, "identity.2fa_enabled", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Ok(new { recoveryCodes });
    }

    private static async Task<IResult> DisableTwoFactorAsync(
        CurrentPasswordRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        var user = await GetUserWithValidPasswordAsync(principal, request.CurrentPassword, userManager);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        EnsureSucceeded(await userManager.SetTwoFactorEnabledAsync(user, false), "Two-factor disable failed");
        EnsureSucceeded(await userManager.ResetAuthenticatorKeyAsync(user), "Authenticator key reset failed");
        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user), "Session revocation failed");
        await signInManager.SignOutAsync();
        database.AuditLogs.Add(new AuditLog(user.Id, "identity.2fa_disabled", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RegenerateRecoveryCodesAsync(
        CurrentPasswordRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        PublishingDbContext database,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var user = await GetUserWithValidPasswordAsync(principal, request.CurrentPassword, userManager);
        if (user is null || !await userManager.GetTwoFactorEnabledAsync(user))
        {
            return Results.Unauthorized();
        }

        var recoveryCodes = (await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))?.ToArray() ?? [];
        database.AuditLogs.Add(new AuditLog(user.Id, "identity.2fa_recovery_codes_regenerated", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Ok(new { recoveryCodes });
    }

    private static async Task<IResult> RevokeAllSessionsAsync(
        CurrentPasswordRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        var user = await GetUserWithValidPasswordAsync(principal, request.CurrentPassword, userManager);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user), "Session revocation failed");
        await signInManager.SignOutAsync();
        database.AuditLogs.Add(new AuditLog(user.Id, "identity.sessions_revoked", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<ApplicationUser?> GetUserWithValidPasswordAsync(
        System.Security.Claims.ClaimsPrincipal principal,
        string password,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(principal);
        return user is not null && user.IsActive && !string.IsNullOrWhiteSpace(password) && await userManager.CheckPasswordAsync(user, password)
            ? user
            : null;
    }

    private static string NormalizeAuthenticatorCode(string code) => code.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"{message}: {string.Join(", ", result.Errors.Select(error => error.Code))}");
        }
    }

    private sealed record LoginRequest(string Email, string Password);
    private sealed record RegisterRequest(string Email,string Password,string DisplayName);
    private sealed record TwoFactorLoginRequest(string? AuthenticatorCode, string? RecoveryCode);
    private sealed record TwoFactorCodeRequest(string Code);
    private sealed record CurrentPasswordRequest(string CurrentPassword);
}
