using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public static class FakeLeaveLimitsDtoProvider
{
    public static IEnumerable<UserLeaveLimitsService.LeaveLimitDto> GetAll(int year)
    {
        return new[]
        {
            new UserLeaveLimitsService.LeaveLimitDto(
                TimeSpan.FromHours(8), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.HolidayLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fakeDesc"), FakeUserProvider.BenId),
            new UserLeaveLimitsService.LeaveLimitDto(
                TimeSpan.FromHours(16), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.OnDemandLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fakeDesc"), FakeUserProvider.BenId),
            new UserLeaveLimitsService.LeaveLimitDto(
                TimeSpan.FromHours(16), TimeSpan.FromHours(24), FakeLeaveTypeDtoProvider.OnDemandLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fakeDesc"), FakeUserProvider.PhilipId),
        };
    }
}