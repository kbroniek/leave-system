using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.UserLeaveLimits;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public static class FakeLeaveLimitsDtoProvider
{
    public static IEnumerable<LeaveLimitDto> GetAll(int year) =>
        new[]
        {
            new LeaveLimitDto(
                Guid.Parse("FE506F33-941D-42C9-B182-73EB6EF43AEC"),TimeSpan.FromHours(8), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.HolidayLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc"), FakeUserProvider.BenId),
            new LeaveLimitDto(
                Guid.Parse("47BAC5B1-0B80-4D32-BDA0-BB716A9EAC26"),TimeSpan.FromHours(16), TimeSpan.FromHours(16), FakeLeaveTypeDtoProvider.OnDemandLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc"), FakeUserProvider.BenId),
            new LeaveLimitDto(
                Guid.Parse("CC28295E-C9DB-4278-AD7C-5CC11885180B"),TimeSpan.FromHours(16), TimeSpan.FromHours(24), FakeLeaveTypeDtoProvider.OnDemandLeaveId,DateTimeOffsetExtensions.GetFirstDayOfYear(year),
                DateTimeOffsetExtensions.GetLastDayOfYear(year), new UserLeaveLimitPropertyDto("fakeDesc"), FakeUserProvider.PhilipId),
        };
}
