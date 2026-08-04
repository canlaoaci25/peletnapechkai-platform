using System.Text.Json;
using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Endpoints;

public static class UserManagementEndpoints
{
    public static IEndpointRouteBuilder MapUserManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin/users")
            .WithTags("User management")
            .RequireAuthorization(AuthorizationPolicies.ManageUsers);

        group.MapGet("/", ListUsersAsync);
        group.MapPost("/invite", InviteUserAsync).ValidateAntiforgery();
        group.MapPut("/{userId:guid}/roles", ChangeRolesAsync).ValidateAntiforgery();
        group.MapPut("/{userId:guid}/active", ChangeActiveStateAsync).ValidateAntiforgery();
        group.MapPost("/{userId:guid}/revoke-sessions", RevokeSessionsAsync).ValidateAntiforgery();

        endpoints.MapPost("/api/v1/auth/complete-invitation", CompleteInvitationAsync)
            .AllowAnonymous()
            .RequireRateLimiting(IdentityServiceExtensions.LoginRateLimitPolicy)
            .ValidateAntiforgery();

        return endpoints;
    }

    private static async Task<IResult> ListUsersAsync(
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var users = await userManager.Users.OrderBy(user => user.Email).ToListAsync(cancellationToken);
        var result = new List<object>(users.Count);
        foreach (var user in users)
        {
            result.Add(new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                user.IsActive,
                user.EmailConfirmed,
                user.TwoFactorEnabled,
                user.LockoutEnd,
                roles = await userManager.GetRolesAsync(user)
            });
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> InviteUserAsync(
        InviteUserRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        PublishingDbContext database,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var requestedRoles = request.Roles ?? [];
        var validation = ValidateRoles(requestedRoles, principal);
        if (validation is not null)
        {
            return validation;
        }

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["user"] = ["Email and display name are required."] });
        }

        var actor = await userManager.GetUserAsync(principal);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var creation = await userManager.CreateAsync(user);
        if (!creation.Succeeded)
        {
            return IdentityValidationProblem(creation);
        }

        var roleResult = await userManager.AddToRolesAsync(user, requestedRoles.Distinct(StringComparer.OrdinalIgnoreCase));
        if (!roleResult.Succeeded)
        {
            return IdentityValidationProblem(roleResult);
        }

        var invitationToken = await userManager.GeneratePasswordResetTokenAsync(user);
        database.AuditLogs.Add(new AuditLog(
            actor.Id,
            "identity.user_invited",
            nameof(ApplicationUser),
            user.Id,
            JsonSerializer.Serialize(new { roles = requestedRoles }),
            DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        context.Response.Headers.CacheControl = "no-store";
        return Results.Created($"/api/v1/admin/users/{user.Id}", new { user.Id, invitationToken });
    }

    private static async Task<IResult> CompleteInvitationAsync(
        CompleteInvitationRequest request,
        UserManager<ApplicationUser> userManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || !user.IsActive || await userManager.HasPasswordAsync(user))
        {
            return Results.BadRequest();
        }

        var result = await userManager.ResetPasswordAsync(user, request.InvitationToken, request.Password);
        if (!result.Succeeded)
        {
            return IdentityValidationProblem(result);
        }

        user.EmailConfirmed = true;
        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            return IdentityValidationProblem(update);
        }

        database.AuditLogs.Add(new AuditLog(user.Id, "identity.invitation_completed", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ChangeRolesAsync(
        Guid userId,
        ChangeRolesRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        var requestedRoles = request.Roles ?? [];
        var validation = ValidateRoles(requestedRoles, principal);
        if (validation is not null)
        {
            return validation;
        }

        var actor = await userManager.GetUserAsync(principal);
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (actor is null || user is null)
        {
            return Results.NotFound();
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(RoleNames.Owner) && !principal.IsInRole(RoleNames.Owner))
        {
            return Results.Forbid();
        }

        if (currentRoles.Contains(RoleNames.Owner) && !requestedRoles.Contains(RoleNames.Owner, StringComparer.OrdinalIgnoreCase) &&
            await CountActiveOwnersAsync(userManager, cancellationToken) <= 1)
        {
            return Results.Conflict(new { message = "The last active Owner role cannot be removed." });
        }

        var desired = requestedRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var remove = currentRoles.Where(role => !desired.Contains(role)).ToArray();
        var add = desired.Where(role => !currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase)).ToArray();
        await using var transaction = await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        EnsureSucceeded(await userManager.RemoveFromRolesAsync(user, remove), "Role removal failed");
        EnsureSucceeded(await userManager.AddToRolesAsync(user, add), "Role assignment failed");
        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user), "Session revocation failed");
        database.AuditLogs.Add(new AuditLog(actor.Id, "identity.roles_changed", nameof(ApplicationUser), user.Id, JsonSerializer.Serialize(new { roles = desired }), DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ChangeActiveStateAsync(
        Guid userId,
        ChangeActiveStateRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        var actor = await userManager.GetUserAsync(principal);
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (actor is null || user is null)
        {
            return Results.NotFound();
        }

        if (!request.IsActive && actor.Id == user.Id)
        {
            return Results.Conflict(new { message = "You cannot deactivate your own account." });
        }

        if (await userManager.IsInRoleAsync(user, RoleNames.Owner) && !principal.IsInRole(RoleNames.Owner))
        {
            return Results.Forbid();
        }

        if (!request.IsActive && await userManager.IsInRoleAsync(user, RoleNames.Owner) &&
            await CountActiveOwnersAsync(userManager, cancellationToken) <= 1)
        {
            return Results.Conflict(new { message = "The last active Owner cannot be deactivated." });
        }

        user.IsActive = request.IsActive;
        EnsureSucceeded(await userManager.UpdateAsync(user), "User update failed");
        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user), "Session revocation failed");
        database.AuditLogs.Add(new AuditLog(actor.Id, request.IsActive ? "identity.user_activated" : "identity.user_deactivated", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RevokeSessionsAsync(
        Guid userId,
        System.Security.Claims.ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        PublishingDbContext database,
        CancellationToken cancellationToken)
    {
        var actor = await userManager.GetUserAsync(principal);
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (actor is null || user is null)
        {
            return Results.NotFound();
        }

        if (await userManager.IsInRoleAsync(user, RoleNames.Owner) && !principal.IsInRole(RoleNames.Owner))
        {
            return Results.Forbid();
        }

        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user), "Session revocation failed");
        database.AuditLogs.Add(new AuditLog(actor.Id, "identity.sessions_revoked_by_admin", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static IResult? ValidateRoles(IEnumerable<string> requestedRoles, System.Security.Claims.ClaimsPrincipal principal)
    {
        var roles = requestedRoles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (roles.Length == 0 || roles.Any(role => !RoleNames.All.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["roles"] = ["At least one valid role is required."] });
        }

        return roles.Contains(RoleNames.Owner, StringComparer.OrdinalIgnoreCase) && !principal.IsInRole(RoleNames.Owner)
            ? Results.Forbid()
            : null;
    }

    private static async Task<int> CountActiveOwnersAsync(UserManager<ApplicationUser> userManager, CancellationToken cancellationToken)
    {
        var owners = await userManager.GetUsersInRoleAsync(RoleNames.Owner);
        var ownerIds = owners.Where(user => user.IsActive).Select(user => user.Id).ToArray();
        return await userManager.Users.CountAsync(user => ownerIds.Contains(user.Id), cancellationToken);
    }

    private static IResult IdentityValidationProblem(IdentityResult result) =>
        Results.ValidationProblem(result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"{message}: {string.Join(", ", result.Errors.Select(error => error.Code))}");
        }
    }

    private sealed record InviteUserRequest(string Email, string DisplayName, string[]? Roles);
    private sealed record CompleteInvitationRequest(Guid UserId, string InvitationToken, string Password);
    private sealed record ChangeRolesRequest(string[]? Roles);
    private sealed record ChangeActiveStateRequest(bool IsActive);
}
