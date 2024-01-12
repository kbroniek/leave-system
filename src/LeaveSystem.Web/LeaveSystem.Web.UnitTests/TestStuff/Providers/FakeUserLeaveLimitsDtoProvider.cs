using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public static class FakeUserLeaveLimitsDtoProvider
{
    public static IEnumerable<UserLeaveLimitDto> GetAllUserLimits(int year)
    {
        return new[]
        {
            new UserLeaveLimitDto(
                Guid.NewGuid(), TimeSpan.FromHours(8), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc")),
            new UserLeaveLimitDto(
                Guid.NewGuid(), TimeSpan.FromHours(16), TimeSpan.FromHours(16),
                FakeLeaveTypeDtoProvider.OnDemandLeaveId, DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc")),
            new UserLeaveLimitDto(
                Guid.NewGuid(), TimeSpan.FromHours(16), TimeSpan.FromHours(24),
                FakeLeaveTypeDtoProvider.OnDemandLeaveId, DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc")),
        };
    }

    public static IEnumerable<LeaveLimitDto> GetAllLimits() =>
        new[]
        {
            new LeaveLimitDto(
                Guid.Parse("099a44f7-4267-48e5-8645-5497b937de0f"),
                TimeSpan.FromHours(8),
                TimeSpan.FromHours(16),
                FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffset.Parse("2024-01-01"),
                DateTimeOffset.Parse("2024-12-31"),
                new UserLeaveLimitPropertyDto("fakeDesc"),
                "1"),
            new LeaveLimitDto(
                Guid.Parse("da8756af-133e-4024-a0f4-866d6624bdaa"),
                TimeSpan.FromHours(16),
                TimeSpan.FromHours(16),
                FakeLeaveTypeDtoProvider.OnDemandLeaveId,
                DateTimeOffset.Parse("2023-01-01"),
                DateTimeOffset.Parse("2023-12-31"),
                new UserLeaveLimitPropertyDto("fakeDesc"),
                "2"),
            new LeaveLimitDto(
                Guid.Parse("0593f540-339a-45ec-8091-044f3e8e568b"),
                TimeSpan.FromHours(16),
                TimeSpan.FromHours(24),
                FakeLeaveTypeDtoProvider.OnDemandLeaveId,
                DateTimeOffset.Parse("2023-03-02"),
                DateTimeOffset.Parse("2023-5-24"),
                new UserLeaveLimitPropertyDto("fakeDesc"),
                "3"),
        };
}