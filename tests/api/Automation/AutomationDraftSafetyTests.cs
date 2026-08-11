using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class AutomationDraftSafetyTests
{
    [Fact]
    public void Automated_non_default_translation_can_publish_and_receive_seo()
    {
        var now = DateTimeOffset.UtcNow;
        var region = new Region(Guid.CreateVersion7(), "FR", "France", "EUR");
        var locale = new Locale(Guid.CreateVersion7(), "fr-FR", "fr", region, "French", "Français", false);
        var article = new ArticleLocalization(new ArticleGroup(ArticleType.Analysis, now), locale, "taslak", "Titre", "Résumé", "<p>Contenu</p>", now);

        article.PublishAutomatedTranslation(now.AddSeconds(1));
        article.UpdateAutomatedSeo("Titre SEO", "Description SEO", now.AddSeconds(2));

        Assert.Equal(PublicationStatus.Published, article.Status);
        Assert.Equal("Titre SEO", article.SeoTitle);
        Assert.Equal(now.AddSeconds(1), article.PublishedAt);
    }

    [Fact]
    public void Default_locale_cannot_use_automated_translation_publish_bypass()
    {
        var now = DateTimeOffset.UtcNow;
        var region = new Region(Guid.CreateVersion7(), "TR", "Türkiye", "TRY");
        var locale = new Locale(Guid.CreateVersion7(), "tr-TR", "tr", region, "Turkish", "Türkçe", true);
        var article = new ArticleLocalization(new ArticleGroup(ArticleType.Analysis, now), locale, "taslak", "Başlık", "Özet", "<p>İçerik</p>", now);

        Assert.Throws<InvalidOperationException>(() => article.PublishAutomatedTranslation(now.AddSeconds(1)));
        Assert.Equal(PublicationStatus.Draft, article.Status);
    }
}
