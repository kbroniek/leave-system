using System;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public class FakeWorkingHoursCreatedProvider
{
    public WorkingHoursCreated GetForBen() => WorkingHoursCreated.Create(
        Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2020, 12, 6),
        DateTimeOffsetExtensions.CreateFromDate(2020, 12, 6), TimeSpan.FromHours(8)
        );
}