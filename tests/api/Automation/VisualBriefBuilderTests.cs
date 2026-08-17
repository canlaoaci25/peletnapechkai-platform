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
        Assert.Equal("technical editorial illustration", brief.VisualType);
        Assert.Contains("technically plausible", brief.Prompt);
    }

    [Fact]
    public void Falls_back_to_summary_when_article_has_no_section_heading()
    {
        var brief = VisualBriefBuilder.Build("Başlık", "Somut özet bağlamı", "<p>Kısa gövde</p>", "en-US", []);
        Assert.Equal("Somut özet bağlamı", brief.SectionContext);
        Assert.Equal("natural editorial photograph", brief.VisualType);
    }

    [Theory]
    [InlineData("Telefon kurulum adımları", "Uygulamayı güvenli biçimde yapılandırın.", "step-by-step editorial illustration")]
    [InlineData("İki dizüstü bilgisayar karşılaştırması", "Modellerin farklarını inceleyin.", "comparison editorial illustration")]
    [InlineData("Kullanım oranları araştırması", "Yeni veriler trendi gösteriyor.", "data-led editorial illustration")]
    public void Selects_information_led_visual_type_instead_of_a_generic_photo(string title, string summary, string expected)
    {
        var brief = VisualBriefBuilder.Build(title, summary, $"<h2>{summary}</h2><p>{new string('x', 2000)}</p>", "tr-TR", ["Teknoloji"]);
        Assert.Equal(expected, brief.VisualType);
        Assert.Contains("entirely text-free", brief.Prompt);
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

    [Fact]
    public void Candidate_requires_every_quality_gate_before_promotion()
    {
        var now = DateTimeOffset.UtcNow; var actor = Guid.NewGuid();
        var task = new VisualReviewTask(Guid.NewGuid(), null, 42, "missing-cover", "Batarya güvenliği", "Hero", "Concrete scene", "No text", "key-2", now);
        task.AttachCandidate(Guid.NewGuid(), "BOECL AI", "BOECL original", null, "Bataryayı inceleyen uzman", 91, 100, 88, 84, Guid.NewGuid(), 16, now);
        Assert.False(task.CandidatePasses);
        Assert.Throws<InvalidOperationException>(() => task.MarkPromoted(actor, "reviewed", now));
        task.AttachCandidate(Guid.NewGuid(), "BOECL AI", "BOECL original", null, "Bataryayı inceleyen uzman", 91, 100, 88, 90, Guid.NewGuid(), 10, now);
        Assert.True(task.CandidatePasses);
        task.MarkPromoted(actor, "Teknik ve editoryal kontrol tamamlandı", now);
        Assert.Equal(VisualReviewStatus.Approved, task.Status); Assert.Equal(now, task.PromotedAt);
    }
}
