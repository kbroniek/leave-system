using System;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Stubs;
using LeaveSystem.UnitTests.TestHelpers;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeLeaveRequestCreatedProvider
{
    internal static Guid FakeLeaveRequestId = Guid.NewGuid();
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;
    private static readonly LeaveRequestCreated FakeLeaveRequestCreatedEvent =
        FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
    
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
    
    internal static MartenQueryableStub<LeaveRequestCreated> GetMartenQueryableStub()
    {
        return new MartenQueryableStub<LeaveRequestCreated>
        {
            LeaveRequestCreated.Create(
                FakeLeaveRequestCreatedEvent.LeaveRequestId,
                FakeLeaveRequestCreatedEvent.DateFrom + (WorkingHours * 3),
                FakeLeaveRequestCreatedEvent.DateTo - (WorkingHours * 2),
                WorkingHours,
                FakeLeaveRequestCreatedEvent.LeaveTypeId,
                FakeLeaveRequestCreatedEvent.Remarks,
                FakeLeaveRequestCreatedEvent.CreatedBy
            ),
            LeaveRequestCreated.Create(
                FakeLeaveRequestCreatedEvent.LeaveRequestId,
                FakeLeaveRequestCreatedEvent.DateFrom + (WorkingHours * 2),
                FakeLeaveRequestCreatedEvent.DateTo - (WorkingHours * 3),
                WorkingHours * 2,
                FakeLeaveRequestCreatedEvent.LeaveTypeId,
                FakeLeaveRequestCreatedEvent.Remarks,
                FakeLeaveRequestCreatedEvent.CreatedBy
            )
        };
    }

    internal static MartenQueryableStub<LeaveRequestCreated> GetLeaveRequestCreatedEventsWithDifferentIds()
    {
        return new MartenQueryableStub<LeaveRequestCreated>
        {
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy
            )
        };
    }
}