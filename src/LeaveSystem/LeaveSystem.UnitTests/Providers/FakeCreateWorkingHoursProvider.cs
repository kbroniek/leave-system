using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeCreateWorkingHoursProvider
{
    public static CreateWorkingHours GetForBen() => CreateWorkingHours.Create(
        Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2020, 12, 1),
        DateTimeOffsetExtensions.CreateFromDate(2025, 12, 1), TimeSpan.FromHours(8), FakeUserProvider.GetUserWithNameFakeoslav()
    );

    public static CreateWorkingHours GetForFakeoslav() => CreateWorkingHours.Create(
        Guid.NewGuid(), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2020, 12, 1),
        DateTimeOffsetExtensions.CreateFromDate(2025, 12, 1), TimeSpan.FromHours(8), FakeUserProvider.GetUserWithNameFakeoslav()
    );
}
