namespace LeaveSystem.Domain.LeaveRequests;
using System;
using System.Collections.Generic;
using LeaveSystem.Domain.EventSourcing;

public interface IReadEventsRepository
{
    IAsyncEnumerable<IEvent> ReadStreamAsync(Guid streamId);
}
