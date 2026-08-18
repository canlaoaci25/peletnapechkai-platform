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

    [Fact]
    public void Builds_a_bounded_section_plan_from_the_full_article()
    {
        var body = string.Join("", Enumerable.Range(1, 7).Select(index =>
            $"<h2>Bölüm {index}</h2><p>{new string((char)('a' + index), 120)}</p>"));
        var plan = VisualBriefBuilder.BuildSectionPlan("Elektrikli araç rehberi", "Pratik bir rehber.", body, "tr-TR", ["Mobilite"]);

        Assert.Equal(3, plan.Length);
        Assert.Equal(["Bölüm 1", "Bölüm 4", "Bölüm 7"], plan.Select(item => item.Heading));
        Assert.All(plan, item => { Assert.Equal(2, item.HeadingLevel); Assert.Contains(item.Heading, item.Prompt); Assert.Contains("text", item.NegativePrompt); });
    }

    [Fact]
    public void Omits_too_thin_sections_from_the_visual_plan()
    {
        Assert.Empty(VisualBriefBuilder.BuildSectionPlan("Başlık", "Özet", "<h2>Boş</h2><p>Kısa</p>", "tr-TR", []));
    }

    [Fact]
    public void Body_section_review_tasks_require_a_stable_heading()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.Throws<ArgumentException>(() => new VisualReviewTask(Guid.NewGuid(), null, 60, "missing-body-visual", "Bölüm", "Section visual", "Prompt", "No text", "body:key", now, target: VisualReviewTarget.BodySection));

        var task = new VisualReviewTask(Guid.NewGuid(), null, 60, "missing-body-visual", "Bölüm", "Section visual", "Prompt", "No text", "body:key:valid", now, target: VisualReviewTarget.BodySection, sectionHeading: "Güvenlik");
        Assert.Equal(VisualReviewTarget.BodySection, task.Target);
        Assert.Equal("Güvenlik", task.SectionHeading);
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
        task.AttachCandidate(Guid.NewGuid(), "BOECL AI", "BOECL original", null, "Bataryayı inceleyen uzman", false, true, true, true, true, true, true, actor, 88, 90, Guid.NewGuid(), 10, now);
        Assert.False(task.CandidatePasses);
        task.AttachCandidate(Guid.NewGuid(), "BOECL AI", "BOECL original", null, "Bataryayı inceleyen uzman", true, true, true, true, true, true, true, actor, 88, 84, Guid.NewGuid(), 16, now);
        Assert.False(task.CandidatePasses);
        Assert.Throws<InvalidOperationException>(() => task.MarkPromoted(actor, "reviewed", now));
        task.AttachCandidate(Guid.NewGuid(), "BOECL AI", "BOECL original", null, "Bataryayı inceleyen uzman", true, true, true, true, true, true, true, actor, 88, 90, Guid.NewGuid(), 10, now);
        Assert.True(task.CandidatePasses);
        task.MarkPromoted(actor, "Teknik ve editoryal kontrol tamamlandı", now);
        Assert.Equal(VisualReviewStatus.Approved, task.Status); Assert.Equal(now, task.PromotedAt);
    }

    [Fact]
    public void Retry_and_rejection_invalidate_stale_candidate_evidence()
    {
        var now = DateTimeOffset.UtcNow; var actor = Guid.NewGuid();
        var task = new VisualReviewTask(Guid.NewGuid(), null, 42, "topic-mismatch", "Bölüm", "Hero", "Prompt", "No text", "key-3", now);
        task.AttachCandidate(Guid.NewGuid(), "Licensed stock", "Editorial licence", null, "Somut sahne", true, true, true, true, true, true, true, actor, 100, 100, null, 0, now);
        Assert.True(task.CandidatePasses);

        task.ChangeStatus(VisualReviewStatus.RetryRequested, actor, "Bölüm eşleşmesi zayıf", now.AddMinutes(1));

        Assert.False(task.CandidatePasses);
        Assert.Null(task.CandidateMediaAssetId);
        Assert.Equal(now.AddMinutes(1), task.ReviewedAt);
        Assert.Throws<InvalidOperationException>(() => task.MarkPromoted(actor, "Eski adayı yayımla", now.AddMinutes(2)));
    }

    [Fact]
    public void Candidate_fails_when_locale_technical_artifact_or_crop_evidence_is_missing()
    {
        var now = DateTimeOffset.UtcNow; var actor = Guid.NewGuid();
        VisualReviewTask Create() => new(Guid.NewGuid(), null, 42, "topic-mismatch", "Bölüm", "Hero", "Prompt", "No text", Guid.NewGuid().ToString(), now);

        var localeMissing = Create();
        localeMissing.AttachCandidate(Guid.NewGuid(), "Provider", "Licence", null, "Somut sahne", true, true, false, true, true, true, true, actor, 100, 100, null, 0, now);
        var technicalMissing = Create();
        technicalMissing.AttachCandidate(Guid.NewGuid(), "Provider", "Licence", null, "Somut sahne", true, true, true, false, true, true, true, actor, 100, 100, null, 0, now);
        var artifactMissing = Create();
        artifactMissing.AttachCandidate(Guid.NewGuid(), "Provider", "Licence", null, "Somut sahne", true, true, true, true, true, false, true, actor, 100, 100, null, 0, now);
        var cropMissing = Create();
        cropMissing.AttachCandidate(Guid.NewGuid(), "Provider", "Licence", null, "Somut sahne", true, true, true, true, true, true, false, actor, 100, 100, null, 0, now);

        Assert.False(localeMissing.CandidatePasses);
        Assert.False(technicalMissing.CandidatePasses);
        Assert.False(artifactMissing.CandidatePasses);
        Assert.False(cropMissing.CandidatePasses);
    }
}
