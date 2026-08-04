using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Infrastructure.Persistence;

public sealed class PublishingDbContext(DbContextOptions<PublishingDbContext> options)
    : DbContext(options)
{
    public DbSet<Region> Regions => Set<Region>();

    public DbSet<Locale> Locales => Set<Locale>();

    public DbSet<ArticleGroup> ArticleGroups => Set<ArticleGroup>();

    public DbSet<ArticleLocalization> ArticleLocalizations => Set<ArticleLocalization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PublishingDbContext).Assembly);
    }
}
