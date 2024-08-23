namespace LeaveSystem.Domain.EventSourcing;

public interface IEvent
{
    Guid StreamId { get; }
}
