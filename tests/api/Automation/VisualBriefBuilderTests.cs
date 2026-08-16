using Peletnapechkai.Api.Domain.Automation;
using Peletnapechkai.Api.Infrastructure.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class VisualBriefBuilderTests
{
    [Fact]
    public void Uses_section_locale_and_text_free_quality_contract()
    {
        var brief = VisualBriefBuilder.Build("Elektrikli otomobilde kış menzili", "Soğuk hava menzili etkiler.",
            "<h2>Batarya ön koşullandırma</h2><p>Şarjdan önce bataryayı ısıtın.</p>", "tr-TR", ["Mobilite"]);

        Assert.Contains("Batarya ön koşullandırma", brief.SectionContext);
        Assert.Contains("contemporary Turkey", brief.Prompt);
        Assert.Contains("mobile-safe", brief.Prompt);
        Assert.Contains("text", brief.NegativePrompt);
        Assert.Contains("watermark", brief.NegativePrompt);
    }

    [Fact]
    public void Falls_back_to_summary_when_article_has_no_section_heading()
    {
        var brief = VisualBriefBuilder.Build("Başlık", "Somut özet bağlamı", "<p>Kısa gövde</p>", "en-US", []);
        Assert.Equal("Somut özet bağlamı", brief.SectionContext);
    }

    [Fact]
    public void Review_decisions_are_auditable_and_retry_is_counted()
    {
        var now = DateTimeOffset.UtcNow;
        var task = new VisualReviewTask(Guid.NewGuid(), null, 45, "text-risk", "Bölüm", "Hero", "Prompt", "No text", "key", now);
        var actor = Guid.NewGuid();
        task.ChangeStatus(VisualReviewStatus.RetryRequested, actor, "Daha somut sahne", now.AddMinutes(1));
        Assert.Equal(VisualReviewStatus.RetryRequested, task.Status);
        Assert.Equal(1, task.AttemptCount);
        Assert.Equal(actor, task.ReviewedByUserId);
        Assert.Equal("Daha somut sahne", task.ReviewerNote);
    }
}
