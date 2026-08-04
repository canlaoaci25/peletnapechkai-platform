namespace Peletnapechkai.Api.Domain.Auditing;

public sealed class AuditLog
{
    private AuditLog() { }

    public AuditLog(Guid? actorUserId, string action, string entityType, Guid entityId, string? detailsJson, DateTimeOffset occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        Id = Guid.CreateVersion7();
        ActorUserId = actorUserId;
        Action = action.Trim();
        EntityType = entityType.Trim();
        EntityId = entityId;
        DetailsJson = detailsJson;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string? DetailsJson { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
}
