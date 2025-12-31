namespace LeaveSystem.Domain.UnitTests.EventSourcing;

using LeaveSystem.Domain.EventSourcing;

internal class FakeEventSource : IEventSource
{
    public Queue<IEvent> PendingEvents { get; private set; } = new Queue<IEvent>();
    public int Version { get; private set; }

    public IEventSource Evolve(IEvent @event)
    {
        ++Version;
        return this;
    }
}
