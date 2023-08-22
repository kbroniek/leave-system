using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public class FakeUserLeaveLimitsDtoProvider
{
    public static readonly Guid FakeLimitForSickLeaveId = Guid.NewGuid();
    public static readonly Guid FakeLimitForOnDemandLeaveId = Guid.NewGuid();
    public static readonly Guid FakeLimitForHolidayLeaveId = Guid.NewGuid();

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
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fakeDesc"), FakeUserProvider.FakseoslavId),
        };
    }
}