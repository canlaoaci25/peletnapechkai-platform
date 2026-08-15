namespace Peletnapechkai.Api.Domain.Automation;

public sealed class AutomaticContentSchedule
{
    private AutomaticContentSchedule() { }
    public AutomaticContentSchedule(Guid actorId, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7(); UpdatedByUserId = actorId; IntervalMinutes = 3; UpdatedAt = now; NextRunAt = now;
    }
    public Guid Id { get; private set; }
    public bool IsEnabled { get; private set; }
    public int IntervalMinutes { get; private set; }
    public Guid UpdatedByUserId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset NextRunAt { get; private set; }
    public DateTimeOffset? LastEnqueuedAt { get; private set; }
    public Guid? LastJobId { get; private set; }
    public void SetEnabled(bool enabled, Guid actorId, DateTimeOffset now)
    { IsEnabled = enabled; UpdatedByUserId = actorId; UpdatedAt = now; if (enabled) NextRunAt = now; }
    public void MarkEnqueued(Guid jobId, DateTimeOffset now)
    { LastJobId = jobId; LastEnqueuedAt = now; NextRunAt = now.AddMinutes(IntervalMinutes); }
}
