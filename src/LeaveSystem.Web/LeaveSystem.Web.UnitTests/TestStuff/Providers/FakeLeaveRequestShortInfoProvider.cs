using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public class FakeLeaveRequestShortInfoProvider
{
    public static IEnumerable<LeaveRequestShortInfo> GetAll(DateTimeOffset baseDate, TimeSpan? workingHours = null) => new[]
        {
            new LeaveRequestShortInfo(
                Guid.Parse("a2eba4b3-e665-4eaa-93ad-5405bef63490"),
                baseDate + TimeSpan.FromDays(3),
                baseDate + TimeSpan.FromDays(6),
                TimeSpan.FromHours(16),
                FakeLeaveTypeProvider.FakeOnDemandLeaveId,
                LeaveRequestStatus.Accepted,
                FakeUserProvider.GetUserWithNameFakeoslav(),
                workingHours ?? TimeSpan.FromHours(8)),
            new LeaveRequestShortInfo(
                Guid.Parse("a2eba4b3-e665-4eaa-93ad-5405bef63491"),
                baseDate + TimeSpan.FromDays(7),
                baseDate + TimeSpan.FromDays(8),
                TimeSpan.FromHours(8),
                FakeLeaveTypeProvider.FakeOnDemandLeaveId,
                LeaveRequestStatus.Accepted,
                FakeUserProvider.GetUserWithNameFakeoslav(),
                workingHours ?? TimeSpan.FromHours(8)),
            new LeaveRequestShortInfo(
                Guid.Parse("a2eba4b3-e665-4eaa-93ad-5405bef63492"),
                baseDate + TimeSpan.FromDays(1),
                baseDate + TimeSpan.FromDays(5),
                TimeSpan.FromHours(16),
                FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
                LeaveRequestStatus.Canceled,
                FakeUserProvider.GetUserBen(),
                workingHours ?? TimeSpan.FromHours(8)),
        };

    public static IEnumerable<LeaveRequestShortInfo> GetAllV2(DateTimeOffset baseDate, TimeSpan? workingHours = null) => new[]
        {
            new LeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate + TimeSpan.FromDays(3),
                baseDate + TimeSpan.FromDays(6),
                TimeSpan.FromHours(16),
                FakeLeaveTypeProvider.FakeOnDemandLeaveId,
                LeaveRequestStatus.Pending,
                FakeUserProvider.GetUserWithNameFakeoslav(),
                workingHours ?? TimeSpan.FromHours(8)),
            new LeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate + TimeSpan.FromDays(7),
                baseDate + TimeSpan.FromDays(8),
                TimeSpan.FromHours(8),
                FakeLeaveTypeProvider.FakeOnDemandLeaveId,
                LeaveRequestStatus.Canceled,
                FakeUserProvider.GetUserWithNameFakeoslav(),
                workingHours ?? TimeSpan.FromHours(8)),
            new LeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate + TimeSpan.FromDays(1),
                baseDate + TimeSpan.FromDays(5),
                TimeSpan.FromHours(4),
                FakeLeaveTypeProvider.FakeSickLeaveId,
                LeaveRequestStatus.Accepted,
                FakeUserProvider.GetUserBen(),
                workingHours ?? TimeSpan.FromHours(8)),
        };
}
