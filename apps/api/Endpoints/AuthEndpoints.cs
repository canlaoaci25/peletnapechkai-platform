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

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .ValidateAntiforgery();

        group.MapGet("/session", SessionAsync).RequireAuthorization();

        return endpoints;
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

    private sealed record LoginRequest(string Email, string Password);
}
