using System;
using LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeCreateWorkingHoursProvider
{
    public static AddWorkingHours GetForBen() => AddWorkingHours.Create(
        Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2020, 12, 1),
        DateTimeOffsetExtensions.CreateFromDate(2025, 12, 1), TimeSpan.FromHours(8), FakeUserProvider.GetUserWithNameFakeoslav()
    );

    public static AddWorkingHours GetForFakeoslav() => AddWorkingHours.Create(
        Guid.NewGuid(), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2020, 12, 1),
        DateTimeOffsetExtensions.CreateFromDate(2025, 12, 1), TimeSpan.FromHours(8), FakeUserProvider.GetUserWithNameFakeoslav()
    );
}