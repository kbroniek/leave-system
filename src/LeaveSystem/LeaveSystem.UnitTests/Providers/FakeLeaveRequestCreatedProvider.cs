using System;
using System.Collections.Generic;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeLeaveRequestCreatedProvider
{
    internal static Guid FakeLeaveRequestId = Guid.NewGuid();
    private static readonly TimeSpan WorkingHours = TimeSpan.FromHours(8);
    private static readonly LeaveRequestCreated FakeLeaveRequestCreatedEvent =
        GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();

    internal static LeaveRequestCreated GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate()
    {
        var now = DateTimeOffset.Now;
        var dateFrom = DateCalculator.GetNextWorkingDay(now + new TimeSpan(2, 0, 0, 0));
        var twoDaysAfterDateFrom = dateFrom + new TimeSpan(2, 0, 0, 0);
        var dateTo = DateCalculator.GetNextWorkingDay(twoDaysAfterDateFrom);
        return LeaveRequestCreated.Create(
            FakeLeaveRequestId,
            dateFrom,
            dateTo,
            TimeSpan.FromDays(6),
            FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav(),
            WorkingHoursUtils.DefaultWorkingHours
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
            FakeUserProvider.GetUserWithNameFakeoslav(),
            WorkingHoursUtils.DefaultWorkingHours
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
            FakeUserProvider.GetUserWithNameFakeoslav(),
            TimeSpan.FromHours(4)
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
            FakeUserProvider.GetUserWithNameFakeoslav(),
            WorkingHoursUtils.DefaultWorkingHours
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
            FakeUserProvider.GetUserWithNameFakeoslav(),
            WorkingHoursUtils.DefaultWorkingHours
        );
    }

    internal static IList<LeaveRequestCreated> GetMartenQueryableStub()
    {
        return new List<LeaveRequestCreated>
        {
            LeaveRequestCreated.Create(
                FakeLeaveRequestCreatedEvent.LeaveRequestId,
                FakeLeaveRequestCreatedEvent.DateFrom + (WorkingHours * 3),
                FakeLeaveRequestCreatedEvent.DateTo - (WorkingHours * 2),
                WorkingHours,
                FakeLeaveRequestCreatedEvent.LeaveTypeId,
                FakeLeaveRequestCreatedEvent.Remarks,
                FakeLeaveRequestCreatedEvent.CreatedBy,
                WorkingHoursUtils.DefaultWorkingHours
            ),
            LeaveRequestCreated.Create(
                FakeLeaveRequestCreatedEvent.LeaveRequestId,
                FakeLeaveRequestCreatedEvent.DateFrom + (WorkingHours * 2),
                FakeLeaveRequestCreatedEvent.DateTo - (WorkingHours * 3),
                WorkingHours * 2,
                FakeLeaveRequestCreatedEvent.LeaveTypeId,
                FakeLeaveRequestCreatedEvent.Remarks,
                FakeLeaveRequestCreatedEvent.CreatedBy,
                WorkingHoursUtils.DefaultWorkingHours
            )
        };
    }

    internal static IList<LeaveRequestCreated> GetLeaveRequestCreatedEventsWithDifferentIds()
    {
        return new List<LeaveRequestCreated>
        {
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
                WorkingHoursUtils.DefaultWorkingHours
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy,
                WorkingHoursUtils.DefaultWorkingHours
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy,WorkingHoursUtils.DefaultWorkingHours
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy,
                WorkingHoursUtils.DefaultWorkingHours
            )
        };
    }
}