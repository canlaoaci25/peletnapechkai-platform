using Peletnapechkai.Api.Domain.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class AutomationJobTests
{
    [Fact]
    public void Job_preserves_unique_target_locales_and_checkpoint_progress()
    {
        var createdAt = DateTimeOffset.UtcNow;
        var job = new AutomationJob(
            AutomationJobType.ContentTranslation,
            ["de-DE", "en-US", "de-DE"],
            10,
            Guid.CreateVersion7(),
            createdAt);

        job.Start(1, createdAt.AddSeconds(1));
        job.ReportProgress(4, 1, 2, "İkinci faz işleniyor.", createdAt.AddSeconds(2));

        Assert.Equal(AutomationJobStatus.Running, job.Status);
        Assert.Equal(["de-DE", "en-US"], job.TargetLocales);
        Assert.Equal(4, job.CompletedItems);
        Assert.Equal(1, job.FailedItems);
        Assert.Equal(2, job.CurrentPhase);
    }

    [Fact]
    public void Paused_job_can_resume_from_its_existing_phase()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new AutomationJob(
            AutomationJobType.SeoLocalization,
            ["tr-TR"],
            5,
            Guid.CreateVersion7(),
            now);

        job.Start(3, now.AddSeconds(1));
        job.Pause(now.AddSeconds(2));
        job.Resume(now.AddSeconds(3));

        Assert.Equal(AutomationJobStatus.Queued, job.Status);
        Assert.Equal(3, job.CurrentPhase);
        Assert.Null(job.CompletedAt);
    }

    [Fact]
    public void Progress_cannot_exceed_total_item_count()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new AutomationJob(
            AutomationJobType.SiteLocalization,
            ["de-DE"],
            1,
            Guid.CreateVersion7(),
            now);
        job.Start(1, now);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            job.ReportProgress(1, 1, 1, null, now));
    }

    [Fact]
    public void Finished_job_rejects_invalid_state_transitions()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new AutomationJob(
            AutomationJobType.SystemReport,
            [],
            1,
            Guid.CreateVersion7(),
            now);
        job.Start(1, now);
        job.Complete(null, "Ayrıntılı sistem raporu", now);

        Assert.Equal(AutomationJobStatus.Completed, job.Status);
        Assert.Equal("Ayrıntılı sistem raporu", job.ReportText);
        Assert.Throws<InvalidOperationException>(() => job.Cancel(now));
        Assert.Throws<InvalidOperationException>(() => job.Pause(now));
    }

    [Fact]
    public void Failed_job_can_be_queued_for_retry()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new AutomationJob(
            AutomationJobType.SystemReport,
            [],
            1,
            Guid.CreateVersion7(),
            now);
        job.Start(1, now);
        job.Fail("CLI options were incompatible.", now);

        job.Retry(now.AddMinutes(1));

        Assert.Equal(AutomationJobStatus.Queued, job.Status);
        Assert.Null(job.CompletedAt);
        Assert.Equal(1, job.CurrentPhase);
    }

    [Fact]
    public void Ready_content_job_preserves_every_requested_phase_option()
    {
        var now=DateTimeOffset.UtcNow;var categoryId=Guid.CreateVersion7();
        var job=new AutomationJob(AutomationJobType.ReadyContentGeneration,["de-DE","en-US"],12,Guid.CreateVersion7(),now);
        job.ConfigureContentGeneration(categoryId,"Guide",true,true,true);
        Assert.Equal(categoryId,job.CategoryId);Assert.Equal("Guide",job.RequestedArticleType);Assert.True(job.IncludeImages);Assert.True(job.AutoTranslate);Assert.True(job.AutoSeo);
    }
}
