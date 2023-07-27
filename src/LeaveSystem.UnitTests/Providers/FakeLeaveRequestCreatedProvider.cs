using System;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeLeaveRequestCreatedProvider
{
    internal static Guid FakeLeaveRequestId = Guid.NewGuid();
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;
    
    internal static LeaveRequestCreated GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate()
    {
        var currentDate = DateTimeOffset.Now;
        var dateFrom = DateCalculator.GetNextWorkingDay(currentDate + new TimeSpan(2, 0, 0, 0));
        var twoDaysAfterDateFrom = dateFrom + new TimeSpan(2, 0, 0, 0);
        var dateTo = DateCalculator.GetNextWorkingDay(twoDaysAfterDateFrom);
        return LeaveRequestCreated.Create(
            FakeLeaveRequestId,
            dateFrom,
            dateTo,
            TimeSpan.FromDays(6),
            FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
    }
    
    internal static LeaveRequestCreated GetLeaveRequestCreatedCalculatedFromCurrentDate(TimeSpan duration, Guid leaveType)
    {
        var currentDate = DateTimeOffset.Now;
        var dateFrom = DateCalculator.GetNextWorkingDay(currentDate + new TimeSpan(2, 0, 0, 0));
        var twoDaysAfterDateFrom = dateFrom + new TimeSpan(2, 0, 0, 0);
        var dateTo = DateCalculator.GetNextWorkingDay(twoDaysAfterDateFrom);
        return LeaveRequestCreated.Create(
            FakeLeaveRequestId,
            dateFrom,
            dateTo,
            duration,
            leaveType,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
    }

    internal static LeaveRequestCreated GetLeaveRequestWithHolidayLeaveCreated()
    {
        return LeaveRequestCreated.Create(
            FakeLeaveRequestId,
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            WorkingHours * 30,
            FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
    }

    internal static LeaveRequestCreated GetLeaveRequestCreatedWithOnDemandLeave()
    {
        return LeaveRequestCreated.Create(
            FakeLeaveRequestId,
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            WorkingHours * 30,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
    }
    
    internal static LeaveRequestCreated GetLeaveRequestCreatedWithSickLeave()
    {
        return LeaveRequestCreated.Create(
            FakeLeaveRequestId,
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            WorkingHours * 30,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
    }
}