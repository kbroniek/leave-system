namespace LeaveSystem.Functions.UnitTests.EventSourcing;

using LeaveSystem.Domain.EventSourcing;

internal record FakeEvent() : IEvent
{
    public Guid StreamId { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
};
