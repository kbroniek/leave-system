namespace LeaveSystem.Domain.EventSourcing;
using System.Collections.Generic;

internal interface IEventSource
{
    IEventSource Evolve(IEvent @event);
    Queue<IEvent> PendingEvents { get; }
}
