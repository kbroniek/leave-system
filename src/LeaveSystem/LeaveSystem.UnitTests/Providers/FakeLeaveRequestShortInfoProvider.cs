using System;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.DeprecatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;

namespace LeaveSystem.UnitTests.Providers;

internal static class FakeLeaveRequestShortInfoProvider
{
    internal static LeaveRequestShortInfo[] Get(DateTimeOffset baseDate)
        => new[]
        {
            CreateLeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate.AddDays(2),
                baseDate.AddDays(5),
                TimeSpan.FromHours(32),
                FakeLeaveTypeProvider.FakeSickLeaveId,
                FakeUserProvider.GetUserBen(),
                TimeSpan.FromHours(8),
                LeaveRequestStatus.Rejected
            ),
            CreateLeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate.AddDays(11),
                baseDate.AddDays(20),
                TimeSpan.FromHours(52),
                FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
                FakeUserProvider.GetUserHabib(),
                TimeSpan.FromHours(4),
                LeaveRequestStatus.Pending
            ),
            CreateLeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate.AddDays(-106),
                baseDate.AddDays(-105),
                TimeSpan.FromHours(16),
                FakeLeaveTypeProvider.FakeSickLeaveId,
                FakeUserProvider.GetUserHabib(),
                TimeSpan.FromHours(4),
                LeaveRequestStatus.Rejected
            ),
            CreateLeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate.AddDays(-32),
                baseDate.AddDays(-29),
                TimeSpan.FromHours(16),
                FakeLeaveTypeProvider.FakeOnDemandLeaveId,
                FakeUserProvider.GetUserPhilip(),
                TimeSpan.FromHours(8),
                LeaveRequestStatus.Deprecated
            ),
            CreateLeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate.AddDays(4),
                baseDate.AddDays(10),
                TimeSpan.FromHours(32),
                FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
                FakeUserProvider.GetUserBen(),
                TimeSpan.FromHours(8),
                LeaveRequestStatus.Accepted
            ),
            CreateLeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate.AddDays(25),
                baseDate.AddDays(28),
                TimeSpan.FromHours(24),
                FakeLeaveTypeProvider.FakeSickLeaveId,
                FakeUserProvider.GetUserWithNameFakeoslav(),
                TimeSpan.FromHours(8),
                LeaveRequestStatus.Accepted
            ),
            CreateLeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate.AddDays(-312),
                baseDate.AddDays(-310),
                TimeSpan.FromHours(16),
                FakeLeaveTypeProvider.FakeOnDemandLeaveId,
                FakeUserProvider.GetUserBen(),
                TimeSpan.FromHours(4),
                LeaveRequestStatus.Canceled
            ),
            CreateLeaveRequestShortInfo(
                Guid.NewGuid(),
                baseDate.AddDays(65),
                baseDate.AddDays(70),
                TimeSpan.FromHours(40),
                FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
                FakeUserProvider.GetUserPhilip(),
                TimeSpan.FromHours(8),
                LeaveRequestStatus.Rejected
            ),
        };


    private static LeaveRequestShortInfo CreateLeaveRequestShortInfo(Guid leaveRequestId, DateTimeOffset dateFrom,
        DateTimeOffset dateTo, TimeSpan duration, Guid leaveTypeId, FederatedUser createdBy, TimeSpan workingHours,
        LeaveRequestStatus status)
    {
        var leaveRequestCreated = LeaveRequestCreated.Create(
            leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, string.Empty, createdBy, workingHours
        );
        var leaveRequestShortInfo = new LeaveRequestShortInfo();
        leaveRequestShortInfo.Apply(leaveRequestCreated);
        switch (status)
        {
            case LeaveRequestStatus.Accepted:
                leaveRequestShortInfo.Apply(LeaveRequestAccepted.Create(leaveRequestId, string.Empty, createdBy));
                break;
            case LeaveRequestStatus.Canceled:
                leaveRequestShortInfo.Apply(LeaveRequestCanceled.Create(leaveRequestId, string.Empty, createdBy));
                break;
            case LeaveRequestStatus.Rejected:
                leaveRequestShortInfo.Apply(LeaveRequestRejected.Create(leaveRequestId, string.Empty, createdBy));
                break;
            case LeaveRequestStatus.Pending:
                break;
            case LeaveRequestStatus.Deprecated:
                leaveRequestShortInfo.Apply(LeaveRequestDeprecated.Create(leaveRequestId, string.Empty, createdBy));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        return leaveRequestShortInfo;
    }
}