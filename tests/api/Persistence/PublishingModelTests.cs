using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Auditing;
using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Domain.Localization;
using Peletnapechkai.Api.Infrastructure.Persistence;

namespace Peletnapechkai.Api.Tests.Persistence;

public sealed class PublishingModelTests
{
    private static PublishingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PublishingDbContext>()
            .UseNpgsql("Host=localhost;Database=model_test;Username=model_test;Password=model_test")
            .Options;

        return new PublishingDbContext(options);
    }

    [Fact]
    public void Model_ContainsCorePublishingEntities()
    {
        using var context = CreateContext();

        Assert.NotNull(context.Model.FindEntityType(typeof(Region)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Locale)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ArticleGroup)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ArticleLocalization)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Category)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Tag)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Author)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Source)));
        Assert.NotNull(context.Model.FindEntityType(typeof(MediaAsset)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ArticleRevision)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SeoMetadata)));
        Assert.NotNull(context.Model.FindEntityType(typeof(AuditLog)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ApplicationUser)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ApplicationRole)));
    }

    [Fact]
    public void IdentitySeed_HasAllDistinctRoles()
    {
        Assert.Equal(RoleNames.All.Length, IdentitySeedData.Roles.Length);
        Assert.Equal(RoleNames.All, IdentitySeedData.Roles.Select(role => role.Name));
        Assert.Equal(IdentitySeedData.Roles.Length, IdentitySeedData.Roles.Select(role => role.Id).Distinct().Count());

    }

    [Fact]
    public void Identity_HasDatabaseUniqueEmailAndUserNameIndexes()
    {
        using var context = CreateContext();

        AssertUniqueIndex(context, typeof(ApplicationUser), "ux_users_normalized_email");
        AssertUniqueIndex(context, typeof(ApplicationUser), "ux_users_normalized_user_name");
        AssertUniqueIndex(context, typeof(ApplicationRole), "ux_roles_normalized_name");
    }

    [Fact]
    public void ArticleLocalization_HasLocaleSlugAndGroupLocaleUniqueIndexes()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(ArticleLocalization));
        Assert.NotNull(entity);

        var uniqueIndexes = entity.GetIndexes()
            .Where(index => index.IsUnique)
            .Select(index => index.GetDatabaseName())
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ux_article_localizations_locale_slug", uniqueIndexes);
        Assert.Contains("ux_article_localizations_group_locale", uniqueIndexes);
    }

    [Fact]
    public void LocaleSeed_HasOneDefaultAndThreeEnabledLocales()
    {
        Assert.Equal(3, SeedData.Locales.Length);
        Assert.Single(SeedData.Locales, locale => locale.IsDefault);
        Assert.All(SeedData.Locales, locale => Assert.True(locale.IsEnabled));
    }

    [Fact]
    public void SupportingContent_HasRequiredUniqueIndexes()
    {
        using var context = CreateContext();

        AssertUniqueIndex(context, typeof(Category), "ux_categories_locale_slug");
        AssertUniqueIndex(context, typeof(Tag), "ux_tags_locale_slug");
        AssertUniqueIndex(context, typeof(Author), "ux_authors_slug");
        AssertUniqueIndex(context, typeof(Source), "ux_sources_url");
        AssertUniqueIndex(context, typeof(MediaAsset), "ux_media_assets_storage_key");
        AssertUniqueIndex(context, typeof(ArticleRevision), "ux_article_revisions_article_number");
    }

    private static void AssertUniqueIndex(PublishingDbContext context, Type entityType, string databaseName)
    {
        var entity = context.Model.FindEntityType(entityType);
        Assert.NotNull(entity);
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.GetDatabaseName() == databaseName);
    }

    [Fact]
    public void AuditLog_CannotBeModified()
    {
        using var context = CreateContext();
        var auditLog = new AuditLog(null, "article.created", "ArticleGroup", Guid.CreateVersion7(), null, DateTimeOffset.UtcNow);
        context.Attach(auditLog).State = EntityState.Modified;

        var exception = Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

        Assert.Equal("Audit log records are append-only.", exception.Message);
    }

    [Fact]
    public void Source_RejectsNonHttpUrls()
    {
        Assert.Throws<ArgumentException>(() =>
            new Source("Local file", new Uri("file:///private/source.txt"), DateTimeOffset.UtcNow));
    }
}
