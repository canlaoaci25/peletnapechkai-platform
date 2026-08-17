using Peletnapechkai.Api.Domain.Content;
using Peletnapechkai.Api.Domain.Identity;
using Peletnapechkai.Api.Domain.Localization;

namespace Peletnapechkai.Api.Tests.Content;

public sealed class ArticleReadingProgressTests
{
    [Fact]
    public void Completion_IsRecordedOnce_AndCannotRegress()
    {
        var started = new DateTimeOffset(2026, 8, 16, 10, 0, 0, TimeSpan.Zero);
        var region = new Region(Guid.NewGuid(), "TR", "Türkiye", "TRY");
        var locale = new Locale(Guid.NewGuid(), "tr-TR", "tr", region, "Türkçe", "Türkçe", true);
        var article = new ArticleLocalization(new ArticleGroup(ArticleType.Analysis, started), locale, "okuma", "Okuma", "Özet", "Gövde", started);
        var progress = new ArticleReadingProgress(new ApplicationUser(), article, 20, "giris", started);

        progress.Update(96, "sonuc", started.AddHours(1));
        var completedAt = progress.CompletedAt;
        progress.Update(10, "giris", started.AddHours(2));

        Assert.Equal(96, progress.Percent);
        Assert.Equal(completedAt, progress.CompletedAt);
        Assert.Equal(started.AddHours(1), progress.CompletedAt);
    }
}
