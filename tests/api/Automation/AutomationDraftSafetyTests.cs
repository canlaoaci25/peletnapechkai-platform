using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class AutomationDraftSafetyTests
{
    [Fact]
    public void Automated_translation_and_seo_remain_draft_until_human_review()
    {
        var now = DateTimeOffset.UtcNow;
        var region = new Region(Guid.CreateVersion7(), "FR", "France", "EUR");
        var locale = new Locale(Guid.CreateVersion7(), "fr-FR", "fr", region, "French", "Français", false);
        var article = new ArticleLocalization(new ArticleGroup(ArticleType.Analysis, now), locale, "taslak", "Titre", "Résumé", "<p>Contenu</p>", now);

        article.UpdateDraft(article.Slug, article.Title, article.Summary, article.Body, "Titre SEO", "Description SEO", now.AddSeconds(1));

        Assert.Equal(PublicationStatus.Draft, article.Status);
        Assert.Throws<InvalidOperationException>(() => article.Publish(now.AddSeconds(2)));
    }
}
