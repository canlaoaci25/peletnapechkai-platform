using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Tests.Persistence;

public sealed class PostgreSqlIntegrationTests
{
    [Fact]
    [Trait("Category", "Database")]
    public async Task MigratedDatabase_HasSeededLocales_AndRuntimeRoleCannotCreateTables()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("RUN_DATABASE_TESTS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
        var connectionString = configuration.GetConnectionString("Database");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var options = new DbContextOptionsBuilder<PublishingDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var context = new PublishingDbContext(options);
        Assert.True(await context.Database.CanConnectAsync());
        Assert.Equal(3, await context.Locales.CountAsync());
        Assert.Equal(3, await context.Regions.CountAsync());
        _ = await context.Categories.CountAsync();
        _ = await context.Tags.CountAsync();
        _ = await context.Authors.CountAsync();
        _ = await context.Sources.CountAsync();
        _ = await context.MediaAssets.CountAsync();
        _ = await context.ArticleRevisions.CountAsync();
        _ = await context.SeoMetadata.CountAsync();
        _ = await context.AuditLogs.CountAsync();
        Assert.Equal(6, await context.Roles.CountAsync());
        Assert.All(await context.Users.ToListAsync(), user =>
        {
            Assert.False(string.IsNullOrWhiteSpace(user.Email));
            Assert.False(string.IsNullOrWhiteSpace(user.SecurityStamp));
        });

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("CREATE TABLE permission_probe(id integer);", connection);
        var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }
}
