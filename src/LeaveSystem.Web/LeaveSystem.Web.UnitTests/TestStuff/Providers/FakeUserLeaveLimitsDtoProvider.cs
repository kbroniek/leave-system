using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public static class FakeUserLeaveLimitsDtoProvider
{
    public static IEnumerable<UserLeaveLimitDto> GetAll(int year)
    {
        return new[]
        {
            new UserLeaveLimitDto(
                Guid.NewGuid(),TimeSpan.FromHours(8), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.HolidayLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc")),
            new UserLeaveLimitDto(
                Guid.NewGuid(),TimeSpan.FromHours(16), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.OnDemandLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc")),
            new UserLeaveLimitDto(
                Guid.NewGuid(),TimeSpan.FromHours(16), TimeSpan.FromHours(24), FakeLeaveTypeDtoProvider.OnDemandLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc")),
        };
    }
}