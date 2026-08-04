using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Peletnapechkai.Api.Infrastructure.Persistence;

public sealed class PublishingDbContextFactory : IDesignTimeDbContextFactory<PublishingDbContext>
{
    public PublishingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<PublishingDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DatabaseMigration")
            ?? "Host=127.0.0.1;Port=5432;Database=peletnapechkai_dev;Username=peletnapechkai_owner;Password=not-configured";

        var options = new DbContextOptionsBuilder<PublishingDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.SetPostgresVersion(18, 4))
            .Options;

        return new PublishingDbContext(options);
    }
}
