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
        Assert.Equal(0, await context.Categories.CountAsync());
        Assert.Equal(0, await context.Tags.CountAsync());
        Assert.Equal(0, await context.Authors.CountAsync());
        Assert.Equal(0, await context.Sources.CountAsync());
        Assert.Equal(0, await context.MediaAssets.CountAsync());
        Assert.Equal(0, await context.ArticleRevisions.CountAsync());
        Assert.Equal(0, await context.SeoMetadata.CountAsync());
        Assert.Equal(0, await context.AuditLogs.CountAsync());
        Assert.Equal(6, await context.Roles.CountAsync());
        Assert.Equal(0, await context.Users.CountAsync());

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("CREATE TABLE permission_probe(id integer);", connection);
        var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }
}
