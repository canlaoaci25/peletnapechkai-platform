using Peletnapechkai.Api.Domain.Automation;

namespace Peletnapechkai.Api.Tests.Automation;

public sealed class AutomationJobTests
{
    [Fact]
    public void Visual_renewal_is_reserved_for_the_checkpointed_editorial_worker()
    {
        Assert.False(AutomationJobType.VisualRenewal.CanBeClaimedByGenericWorker());
        Assert.True(AutomationJobType.ReadyContentGeneration.CanBeClaimedByGenericWorker());
        Assert.True(AutomationJobType.ContentTranslation.CanBeClaimedByGenericWorker());
    }

    [Fact]
    public void Visual_renewal_batch_can_pause_and_resume_without_losing_checkpoint()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new AutomationJob(AutomationJobType.VisualRenewal, ["tr-TR", "en-US"], 12, Guid.CreateVersion7(), now);
        job.Start(1, now.AddSeconds(1));
        job.ReportProgress(5, 1, 2, "Six visuals reviewed.", now.AddSeconds(2));
        job.Pause(now.AddSeconds(3));
        job.Resume(now.AddSeconds(4));

        Assert.Equal(AutomationJobStatus.Queued, job.Status);
        Assert.Equal(5, job.CompletedItems);
        Assert.Equal(1, job.FailedItems);
        Assert.Equal(2, job.CurrentPhase);
    }

    [Fact]
    public void Stale_visual_run_can_be_requeued_without_losing_checkpoint()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new AutomationJob(AutomationJobType.VisualRenewal, ["tr-TR"], 12, Guid.CreateVersion7(), now);
        job.Start(2, now.AddSeconds(1));
        job.ReportProgress(5, 1, 3, "Altı kayıt işlendi.", now.AddSeconds(2));

        job.RecoverStaleRun(now.AddMinutes(20));

        Assert.Equal(AutomationJobStatus.Queued, job.Status);
        Assert.Equal(5, job.CompletedItems);
        Assert.Equal(1, job.FailedItems);
        Assert.Equal(3, job.CurrentPhase);
        Assert.Contains("checkpoint", job.LastMessage);
    }

    [Fact]
    public void Visual_checkpoint_tracks_editorial_outcomes_and_completes_the_batch()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new AutomationJob(AutomationJobType.VisualRenewal, ["tr-TR"], 3, Guid.CreateVersion7(), now);
        job.Start(1, now);

        job.ReconcileVisualCheckpoint(1, 1, 1, now.AddMinutes(1));

        Assert.Equal(AutomationJobStatus.Running, job.Status);
        Assert.Equal(1, job.CompletedItems);
        Assert.Equal(1, job.FailedItems);
        Assert.Contains("2/3", job.LastMessage);

        job.ReconcileVisualCheckpoint(2, 1, 0, now.AddMinutes(2));

        Assert.Equal(AutomationJobStatus.Completed, job.Status);
        Assert.Equal(2, job.CompletedItems);
        Assert.Equal(1, job.FailedItems);
        Assert.Equal(4, job.CurrentPhase);
        Assert.Equal(now.AddMinutes(2), job.CompletedAt);
    }

    [Fact]
    public void Visual_checkpoint_rejects_inconsistent_totals()
    {
        var job = new AutomationJob(AutomationJobType.VisualRenewal, ["tr-TR"], 3, Guid.CreateVersion7(), DateTimeOffset.UtcNow);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            job.ReconcileVisualCheckpoint(1, 0, 1, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Non_visual_or_non_running_jobs_cannot_use_stale_recovery()
    {
        var now = DateTimeOffset.UtcNow;
        var visual = new AutomationJob(AutomationJobType.VisualRenewal, ["tr-TR"], 1, Guid.CreateVersion7(), now);
        var generic = new AutomationJob(AutomationJobType.SystemReport, [], 1, Guid.CreateVersion7(), now);
        generic.Start(1, now);

        Assert.Throws<InvalidOperationException>(() => visual.RecoverStaleRun(now));
        Assert.Throws<InvalidOperationException>(() => generic.RecoverStaleRun(now));
    }

    [Fact]
    public void Ready_content_job_can_be_marked_as_automatically_scheduled()
    {
        var job = new AutomationJob(AutomationJobType.ReadyContentGeneration, ["de-DE"], 1, Guid.CreateVersion7(), DateTimeOffset.UtcNow);
        job.ConfigureContentGeneration(Guid.CreateVersion7(), "Guide", true, true, true);
        job.MarkAutomaticallyScheduled();
        Assert.True(job.IsAutomaticallyScheduled);
        Assert.True(job.IncludeImages);
        Assert.True(job.AutoTranslate);
        Assert.True(job.AutoSeo);
    }

    [Fact]
    public void Automatic_schedule_uses_three_minute_interval()
    {
        var now = DateTimeOffset.UtcNow;
        var schedule = new AutomaticContentSchedule(Guid.CreateVersion7(), now);
        schedule.SetEnabled(true, Guid.CreateVersion7(), now);
        schedule.MarkEnqueued(Guid.CreateVersion7(), now);
        Assert.True(schedule.IsEnabled);
        Assert.Equal(3, schedule.IntervalMinutes);
        Assert.Equal(now.AddMinutes(3), schedule.NextRunAt);
    }

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
    public void Retry_clears_failure_count_without_losing_checkpoint_phase()
    {
        var now = DateTimeOffset.UtcNow;
        var job = new AutomationJob(AutomationJobType.ContentTranslation, ["de-DE"], 5, Guid.CreateVersion7(), now);
        job.Start(1, now);
        job.ReportProgress(2, 1, 3, "Bir kayıt başarısız.", now);
        job.Fail("Geçici hata", now);

        job.Retry(now.AddMinutes(1));

        Assert.Equal(0, job.FailedItems);
        Assert.Equal(2, job.CompletedItems);
        Assert.Equal(3, job.CurrentPhase);
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
