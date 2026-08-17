namespace Peletnapechkai.Api.Domain.Localization;

public enum LocalizationAssignmentStatus { Open, InProgress, Completed }

public sealed class LocalizationAssignment
{
    private LocalizationAssignment() { }
    public LocalizationAssignment(Guid articleGroupId, Guid targetLocaleId, Guid assigneeUserId, DateTimeOffset dueAt, Guid createdByUserId, DateTimeOffset now)
    {
        if (articleGroupId == Guid.Empty || targetLocaleId == Guid.Empty || assigneeUserId == Guid.Empty) throw new ArgumentException("Assignment references are required.");
        if (dueAt <= now) throw new ArgumentOutOfRangeException(nameof(dueAt));
        Id = Guid.CreateVersion7(); ArticleGroupId = articleGroupId; TargetLocaleId = targetLocaleId; AssigneeUserId = assigneeUserId;
        DueAt = dueAt; CreatedByUserId = createdByUserId; Status = LocalizationAssignmentStatus.Open; CreatedAt = now; UpdatedAt = now;
    }
    public Guid Id { get; private set; }
    public Guid ArticleGroupId { get; private set; }
    public Guid TargetLocaleId { get; private set; }
    public Guid AssigneeUserId { get; private set; }
    public DateTimeOffset DueAt { get; private set; }
    public LocalizationAssignmentStatus Status { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public void Assign(Guid assigneeUserId, DateTimeOffset dueAt, DateTimeOffset now) { if (assigneeUserId == Guid.Empty || dueAt <= now) throw new ArgumentException("A valid owner and future due date are required."); AssigneeUserId = assigneeUserId; DueAt = dueAt; Status = LocalizationAssignmentStatus.InProgress; UpdatedAt = now; }
}
