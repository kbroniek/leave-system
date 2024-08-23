namespace LeaveSystem.Domain.EventSourcing;
using System;
using System.Collections.Generic;

public interface IReadEventsRepository
{
    IAsyncEnumerable<IEvent> ReadStreamAsync(Guid streamId, CancellationToken cancellationToken);
}
