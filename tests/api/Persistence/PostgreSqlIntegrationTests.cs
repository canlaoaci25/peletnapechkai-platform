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

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("CREATE TABLE permission_probe(id integer);", connection);
        var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }
}
