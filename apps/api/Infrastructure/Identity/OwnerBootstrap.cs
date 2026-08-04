using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Infrastructure.Identity;

public static class OwnerBootstrap
{
    private const string Command = "--bootstrap-owner";

    public static async Task<bool> TryRunAsync(WebApplication app, string[] args)
    {
        if (!args.Contains(Command, StringComparer.Ordinal))
        {
            return false;
        }

        var email = app.Configuration["OwnerBootstrap:Email"];
        var password = app.Configuration["OwnerBootstrap:Password"];
        var displayName = app.Configuration["OwnerBootstrap:DisplayName"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(displayName))
        {
            throw new InvalidOperationException("Owner bootstrap values are not configured.");
        }

        await using var scope = app.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<PublishingDbContext>();
        if (await database.Users.AnyAsync())
        {
            throw new InvalidOperationException("Owner bootstrap is allowed only when no users exist.");
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await using var transaction = await database.Database.BeginTransactionAsync();
        var user = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            DisplayName = displayName.Trim(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var creation = await userManager.CreateAsync(user, password);
        EnsureSucceeded(creation, "Owner creation failed");
        var roleAssignment = await userManager.AddToRoleAsync(user, RoleNames.Owner);
        EnsureSucceeded(roleAssignment, "Owner role assignment failed");

        database.AuditLogs.Add(new AuditLog(user.Id, "identity.owner_bootstrapped", nameof(ApplicationUser), user.Id, null, DateTimeOffset.UtcNow));
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"{message}: {string.Join(", ", result.Errors.Select(error => error.Code))}");
        }
    }
}
