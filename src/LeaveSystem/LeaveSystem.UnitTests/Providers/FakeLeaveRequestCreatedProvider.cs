using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using System;
using System.Collections.Generic;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeLeaveRequestCreatedProvider
{
    internal static Guid FakeLeaveRequestId = Guid.Parse("ebfd08ea-d8ec-4587-81f5-3dad479b7a8c");
    private static readonly TimeSpan WorkingHours = WorkingHoursUtils.DefaultWorkingHours;
    private static readonly LeaveRequestCreated FakeLeaveRequestCreatedEvent =
        GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();

    internal static LeaveRequestCreated GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate(Guid? leaveRequestId = null)
    {
        var now = DateTimeOffset.Now;
        var dateFrom = DateCalculator.GetNextWorkingDay(now.AddDays(2));
        var twoDaysAfterDateFrom = dateFrom.AddDays(2);
        var dateTo = DateCalculator.GetNextWorkingDay(twoDaysAfterDateFrom);
        return LeaveRequestCreated.Create(
            leaveRequestId ?? FakeLeaveRequestId,
            dateFrom,
            dateTo,
            TimeSpan.FromDays(6),
            FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav(),
            WorkingHoursUtils.DefaultWorkingHours
        );
    }

    internal static LeaveRequestCreated GetHolodayLeaveRequest(TimeSpan duration, Guid leaveType)
    {
        var currentDate = DateTimeOffset.Now;
        var dateFrom = DateCalculator.GetNextWorkingDay(currentDate.AddDays(2));
        var twoDaysAfterDateFrom = dateFrom.AddDays(2);
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

    internal static LeaveRequestCreated GetHolidayLeaveRequest(TimeSpan? duration = null)
    {
        return LeaveRequestCreated.Create(
            FakeLeaveRequestId,
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            duration ?? WorkingHours * 30,
            FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav(),
            TimeSpan.FromHours(4)
        );
    }

    internal static LeaveRequestCreated GetOnDemandLeaveRequest()
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

    internal static LeaveRequestCreated GetSickLeaveRequest(TimeSpan? duration = null)
    {
        return LeaveRequestCreated.Create(
            FakeLeaveRequestId,
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            duration ?? WorkingHours * 30,
            FakeLeaveTypeProvider.FakeSickLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav(),
            WorkingHoursUtils.DefaultWorkingHours
        );
    }

    internal static IList<LeaveRequestCreated> GetMartenQueryableStub(Guid? leaveTypeId = null)
    {
        return new List<LeaveRequestCreated>
        {
            LeaveRequestCreated.Create(
                Guid.Parse("ebfd08ea-d8ec-4587-81f5-3dad479b7a81"),
                FakeLeaveRequestCreatedEvent.DateFrom + (WorkingHours * 3),
                FakeLeaveRequestCreatedEvent.DateTo - (WorkingHours * 2),
                WorkingHours,
                leaveTypeId ?? FakeLeaveRequestCreatedEvent.LeaveTypeId,
                FakeLeaveRequestCreatedEvent.Remarks,
                FakeLeaveRequestCreatedEvent.CreatedBy,
                WorkingHoursUtils.DefaultWorkingHours
            ),
            LeaveRequestCreated.Create(
                Guid.Parse("ebfd08ea-d8ec-4587-81f5-3dad479b7a82"),
                FakeLeaveRequestCreatedEvent.DateFrom + (WorkingHours * 2),
                FakeLeaveRequestCreatedEvent.DateTo - (WorkingHours * 3),
                WorkingHours * 2,
                leaveTypeId ?? FakeLeaveRequestCreatedEvent.LeaveTypeId,
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
                Guid.Parse("8b8b2922-8b14-4a9a-843f-bb341f1938a1"),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.Parse("59689db1-10dc-492e-9da8-00628cb1a701"),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
                WorkingHoursUtils.DefaultWorkingHours
            ),
            LeaveRequestCreated.Create(
                Guid.Parse("8b8b2922-8b14-4a9a-843f-bb341f1938a2"),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.Parse("59689db1-10dc-492e-9da8-00628cb1a702"),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy,
                WorkingHoursUtils.DefaultWorkingHours
            ),
            LeaveRequestCreated.Create(
                Guid.Parse("8b8b2922-8b14-4a9a-843f-bb341f1938a3"),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.Parse("59689db1-10dc-492e-9da8-00628cb1a703"),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy,WorkingHoursUtils.DefaultWorkingHours
            ),
            LeaveRequestCreated.Create(
                Guid.Parse("8b8b2922-8b14-4a9a-843f-bb341f1938a4"),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.Parse("59689db1-10dc-492e-9da8-00628cb1a704"),
                "fake remarks",
                FakeLeaveRequestCreatedEvent.CreatedBy,
                WorkingHoursUtils.DefaultWorkingHours
            )
        };
    }
}