namespace LeaveSystem.Functions.EventSourcing;
public abstract class Event
{
    public abstract Guid StreamId { get; }
    public DateTimeOffset CreatedAt { get; set; }
}
