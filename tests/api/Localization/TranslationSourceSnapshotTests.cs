using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Localization;

public sealed class TranslationSourceSnapshotTests
{
    [Fact]
    public void Snapshot_reports_only_source_fields_changed_after_translation()
    {
        var now = DateTimeOffset.UtcNow;
        var region = new Region(Guid.NewGuid(), "TR", "Türkiye", "TRY");
        var sourceLocale = new Locale(Guid.NewGuid(), "tr-TR", "tr", region, "Turkish", "Türkçe", true);
        var targetLocale = new Locale(Guid.NewGuid(), "en-US", "en", region, "English", "English", false);
        var group = new ArticleGroup(ArticleType.Analysis, now);
        var source = new ArticleLocalization(group, sourceLocale, "kaynak", "Başlık", "Özet", "<p>Gövde</p>", now);
        source.UpdateDraft("kaynak", "Başlık", "Özet", "<p>Gövde</p>", "SEO başlığı", "SEO özeti", now);
        var translation = new ArticleLocalization(group, targetLocale, "source", "Title", "Summary", "<p>Body</p>", now);

        translation.CaptureSourceSnapshot(source);
        source.UpdateDraft("kaynak", "Yeni başlık", "Özet", "<p>Yeni gövde</p>", "SEO başlığı", "SEO özeti", now.AddMinutes(1));

        Assert.Equal(now, translation.SourceSnapshotUpdatedAt);
        Assert.Equal(["Title", "Body"], translation.ChangedSourceFields(source));
    }

    [Fact]
    public void Snapshot_rejects_unrelated_or_default_locale_content()
    {
        var now = DateTimeOffset.UtcNow;
        var region = new Region(Guid.NewGuid(), "TR", "Türkiye", "TRY");
        var locale = new Locale(Guid.NewGuid(), "tr-TR", "tr", region, "Turkish", "Türkçe", true);
        var first = new ArticleLocalization(new ArticleGroup(ArticleType.News, now), locale, "bir", "Bir", "", "", now);
        var second = new ArticleLocalization(new ArticleGroup(ArticleType.News, now), locale, "iki", "İki", "", "", now);

        Assert.Throws<InvalidOperationException>(() => second.CaptureSourceSnapshot(first));
    }
}
