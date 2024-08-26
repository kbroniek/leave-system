namespace LeaveSystem.Functions.UnitTests.TestHelpers;

using System;
using LeaveSystem.Functions.EventSourcing;

internal class FakeEvent : Event
{
    public override Guid StreamId => Id;
    public Guid Id { get; set; }
}
