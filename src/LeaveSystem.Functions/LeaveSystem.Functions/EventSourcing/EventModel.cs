namespace LeaveSystem.Functions.EventSourcing;
using System;

internal record EventModel<TEvent>(
    Guid Id,
    Guid StreamId,
    TEvent Body,
    string EventType)
{
}
