using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public class FakeLeaveRequestShortInfoProvider
{
    public static IEnumerable<LeaveRequestShortInfo> GetAll(DateTimeOffset baseDate)
    {
        return new[]
        {
            new LeaveRequestShortInfo(
                Guid.NewGuid(), 
                baseDate + TimeSpan.FromDays(3),
                baseDate + TimeSpan.FromDays(6),
                TimeSpan.FromHours(16),
                FakeLeaveTypeProvider.FakeOnDemandLeaveId,
                LeaveRequestStatus.Accepted,
                FakeUserProvider.GetUserWithNameFakeoslav()),
            new LeaveRequestShortInfo(
                Guid.NewGuid(), 
                baseDate + TimeSpan.FromDays(7),
                baseDate + TimeSpan.FromDays(8),
                TimeSpan.FromHours(8),
                FakeLeaveTypeProvider.FakeOnDemandLeaveId,
                LeaveRequestStatus.Accepted,
                FakeUserProvider.GetUserWithNameFakeoslav()),
            new LeaveRequestShortInfo(
                Guid.NewGuid(), 
                baseDate + TimeSpan.FromDays(1),
                baseDate + TimeSpan.FromDays(5),
                TimeSpan.FromHours(16),
                FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
                LeaveRequestStatus.Canceled,
                FakeUserProvider.GetUserBen()),
        };
    }
}