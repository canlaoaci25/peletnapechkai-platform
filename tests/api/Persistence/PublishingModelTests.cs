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
        Assert.NotNull(context.Model.FindEntityType(typeof(LocaleCountry)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ArticleGroup)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ArticleLocalization)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Peletnapechkai.Api.Domain.Knowledge.KnowledgeArticleLink)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Category)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Tag)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Author)));
        Assert.NotNull(context.Model.FindEntityType(typeof(Source)));
        Assert.NotNull(context.Model.FindEntityType(typeof(MediaAsset)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ArticleRevision)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ArticleCorrection)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SeoMetadata)));
        Assert.NotNull(context.Model.FindEntityType(typeof(AuditLog)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ApplicationUser)));
        Assert.NotNull(context.Model.FindEntityType(typeof(ApplicationRole)));
        Assert.NotNull(context.Model.FindEntityType(typeof(SavedArticle)));
        Assert.NotNull(context.Model.FindEntityType(typeof(FollowedCategory)));
        Assert.NotNull(context.Model.FindEntityType(typeof(WebPushSubscription)));
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
    public void LocaleSeed_HasOneDefaultAndEverySupportedLocale()
    {
        Assert.Equal(new[] { "de-DE", "en-US", "fr-FR", "tr-TR" }, SeedData.Locales.Select(locale => locale.Code).Order());
        Assert.Equal(4, SeedData.Regions.Length);
        Assert.Single(SeedData.Locales, locale => locale.IsDefault);
        Assert.All(SeedData.Locales, locale => Assert.True(locale.IsEnabled));
    }

    [Fact]
    public void SavedArticle_HasPerMemberArticleUniqueIndex()
    {
        using var context = CreateContext();
        AssertUniqueIndex(context, typeof(SavedArticle), "ux_saved_articles_user_article");
    }

    [Fact]
    public void FollowedCategory_HasPerMemberCategoryUniqueIndex()
    {
        using var context = CreateContext();
        AssertUniqueIndex(context, typeof(FollowedCategory), "ux_followed_categories_user_category");
    }

    [Fact]
    public void WebPushSubscription_HasUniqueEndpointAndRejectsUnsafeEndpoint()
    {
        using var context = CreateContext();
        AssertUniqueIndex(context, typeof(WebPushSubscription), "ux_web_push_subscriptions_endpoint");
        var user = new ApplicationUser { Id = Guid.CreateVersion7(), Email = "member@example.com", DisplayName = "Member", CreatedAt = DateTimeOffset.UtcNow };
        Assert.Throws<ArgumentException>(() => new WebPushSubscription(user, "http://push.example/test", "key", "auth", "tr-TR", 22, 7, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void SupportingContent_HasRequiredUniqueIndexes()
    {
        using var context = CreateContext();

        AssertUniqueIndex(context, typeof(Category), "ux_categories_locale_slug");
        Assert.Contains(context.Model.FindEntityType(typeof(Category))!.GetIndexes(), index => index.GetDatabaseName() == "ix_categories_parent_name");
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

    [Theory]
    [InlineData("http://localhost/report")]
    [InlineData("https://192.168.1.20/report")]
    [InlineData("https://user:secret@example.org/report")]
    [InlineData("https://intranet/report")]
    public void Source_RejectsNonPublicUrls(string value)
    {
        Assert.Throws<ArgumentException>(() =>
            new Source("Güvenilmeyen kaynak", new Uri(value), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Source_NormalizesFragmentsForStableDeduplication()
    {
        var source = new Source("Araştırma", new Uri("https://example.org/report#section"), DateTimeOffset.UtcNow);

        Assert.Equal("https://example.org/report", source.Url);
    }
}
