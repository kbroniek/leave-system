using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public static class FakeUserLeaveLimitsDtoProvider
{
    public static IEnumerable<UserLeaveLimitsService.UserLeaveLimitDto> GetAll(int year)
    {
        return new[]
        {
            new UserLeaveLimitsService.UserLeaveLimitDto(
                TimeSpan.FromHours(8), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.HolidayLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fakeDesc")),
            new UserLeaveLimitsService.UserLeaveLimitDto(
                TimeSpan.FromHours(16), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.OnDemandLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fakeDesc")),
            new UserLeaveLimitsService.UserLeaveLimitDto(
                TimeSpan.FromHours(16), TimeSpan.FromHours(24), FakeLeaveTypeDtoProvider.OnDemandLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fakeDesc")),
        };
    }
}