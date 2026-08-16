namespace Peletnapechkai.Api.Domain.Automation;

public enum AutomationJobType
{
    ContentTranslation,
    SeoLocalization,
    SiteLocalization,
    SystemReport,
    ReadyContentGeneration,
    CategoryLocalization
}

public enum AutomationJobStatus
{
    Queued,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public sealed class AutomationJob
{
    private AutomationJob() { }

    public AutomationJob(AutomationJobType type, IEnumerable<string> targetLocales, int totalItems, Guid actorId, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(totalItems);
        Id = Guid.CreateVersion7();
        Type = type;
        TargetLocales = targetLocales.Where(locale => !string.IsNullOrWhiteSpace(locale)).Select(locale => locale.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
        TotalItems = totalItems;
        CreatedByUserId = actorId;
        Status = AutomationJobStatus.Queued;
        CreatedAt = now;
        UpdatedAt = now;
        LastMessage = "İş kalıcı kuyruğa alındı.";
    }

    public Guid Id { get; private set; }
    public AutomationJobType Type { get; private set; }
    public AutomationJobStatus Status { get; private set; }
    public string[] TargetLocales { get; private set; } = [];
    public int TotalItems { get; private set; }
    public int CompletedItems { get; private set; }
    public int FailedItems { get; private set; }
    public int CurrentPhase { get; private set; }
    public string? LastMessage { get; private set; }
    public string? ReportText { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string? RequestedArticleType { get; private set; }
    public bool IncludeImages { get; private set; }
    public bool AutoTranslate { get; private set; }
    public bool AutoSeo { get; private set; }
    public bool IsAutomaticallyScheduled { get; private set; }

    public void ConfigureContentGeneration(Guid categoryId, string articleType, bool includeImages, bool autoTranslate, bool autoSeo)
    {
        if (Type != AutomationJobType.ReadyContentGeneration || Status != AutomationJobStatus.Queued)
            throw new InvalidOperationException("Only queued ready-content jobs can be configured.");
        if (categoryId == Guid.Empty) throw new ArgumentException("Category is required.", nameof(categoryId));
        ArgumentException.ThrowIfNullOrWhiteSpace(articleType);
        CategoryId = categoryId;
        RequestedArticleType = articleType.Trim();
        IncludeImages = includeImages;
        AutoTranslate = autoTranslate;
        AutoSeo = autoSeo;
    }

    public void MarkAutomaticallyScheduled()
    {
        if (Type != AutomationJobType.ReadyContentGeneration || Status != AutomationJobStatus.Queued)
            throw new InvalidOperationException("Only queued ready-content jobs can be marked automatic.");
        IsAutomaticallyScheduled = true;
    }

    public void Pause(DateTimeOffset now)
    {
        if (Status is not (AutomationJobStatus.Queued or AutomationJobStatus.Running)) throw new InvalidOperationException("Only queued or running jobs can be paused.");
        Status = AutomationJobStatus.Paused;
        LastMessage = "İş kullanıcı tarafından durduruldu.";
        UpdatedAt = now;
    }

    public void Resume(DateTimeOffset now)
    {
        if (Status != AutomationJobStatus.Paused) throw new InvalidOperationException("Only paused jobs can be resumed.");
        Status = AutomationJobStatus.Queued;
        LastMessage = "İş kaldığı fazdan devam etmek üzere kuyruğa alındı.";
        UpdatedAt = now;
    }

    public void Retry(DateTimeOffset now)
    {
        if (Status != AutomationJobStatus.Failed) throw new InvalidOperationException("Only failed jobs can be retried.");
        Status = AutomationJobStatus.Queued;
        FailedItems = 0;
        CompletedAt = null;
        LastMessage = "Hatalı iş yeniden denenmek üzere kuyruğa alındı.";
        UpdatedAt = now;
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status is AutomationJobStatus.Completed or AutomationJobStatus.Cancelled) throw new InvalidOperationException("Completed or cancelled jobs cannot be cancelled again.");
        Status = AutomationJobStatus.Cancelled;
        LastMessage = "İş kullanıcı tarafından iptal edildi.";
        UpdatedAt = now;
        CompletedAt = now;
    }

    public void Start(int phase, DateTimeOffset now)
    {
        if (Status != AutomationJobStatus.Queued) throw new InvalidOperationException("Only queued jobs can start.");
        Status = AutomationJobStatus.Running;
        CurrentPhase = Math.Max(1, phase);
        LastMessage = "Faz çalışıyor.";
        UpdatedAt = now;
    }

    public void ReportProgress(int completed, int failed, int phase, string? message, DateTimeOffset now)
    {
        if (Status != AutomationJobStatus.Running) throw new InvalidOperationException("Only running jobs can report progress.");
        if (completed < 0 || failed < 0 || completed + failed > TotalItems) throw new ArgumentOutOfRangeException(nameof(completed));
        CompletedItems = completed;
        FailedItems = failed;
        CurrentPhase = Math.Max(phase, CurrentPhase);
        LastMessage = string.IsNullOrWhiteSpace(message) ? LastMessage : message.Trim();
        UpdatedAt = now;
    }

    public void Heartbeat(string message, DateTimeOffset now)
    {
        if (Status != AutomationJobStatus.Running) throw new InvalidOperationException("Only running jobs can report a heartbeat.");
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        LastMessage = message.Trim()[..Math.Min(message.Trim().Length, 500)];
        UpdatedAt = now;
    }

    public void Complete(string? message, string? reportText, DateTimeOffset now)
    {
        if (Status != AutomationJobStatus.Running) throw new InvalidOperationException("Only running jobs can complete.");
        Status = AutomationJobStatus.Completed;
        CompletedItems = TotalItems - FailedItems;
        LastMessage = string.IsNullOrWhiteSpace(message) ? "İş tamamlandı." : message.Trim();
        SetReport(reportText, now);
        UpdatedAt = now;
        CompletedAt = now;
    }

    public void SetReport(string? reportText, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(reportText)) return;
        ReportText = reportText.Trim();
        UpdatedAt = now;
    }

    public void Fail(string message, DateTimeOffset now)
    {
        if (Status is AutomationJobStatus.Completed or AutomationJobStatus.Cancelled) throw new InvalidOperationException("A finished job cannot fail.");
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Status = AutomationJobStatus.Failed;
        LastMessage = message.Trim();
        UpdatedAt = now;
        CompletedAt = now;
    }
}
