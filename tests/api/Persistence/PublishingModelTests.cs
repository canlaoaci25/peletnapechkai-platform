using Microsoft.EntityFrameworkCore;
using Peletnapechkai.Api.Domain.Content;
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
}
