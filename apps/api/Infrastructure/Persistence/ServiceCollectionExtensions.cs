using Microsoft.EntityFrameworkCore;

namespace Peletnapechkai.Api.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

        services.AddDbContext<PublishingDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.SetPostgresVersion(18, 4)));

        services.AddHealthChecks().AddDbContextCheck<PublishingDbContext>("postgresql");

        return services;
    }
}
