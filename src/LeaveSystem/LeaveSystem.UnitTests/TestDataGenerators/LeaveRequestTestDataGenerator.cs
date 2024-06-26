﻿using System;
using System.Collections.Generic;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;

namespace LeaveSystem.UnitTests.TestDataGenerators;

public static class LeaveRequestTestDataGenerator
{
    public static IEnumerable<object[]> GetCancelAcceptAndRejectMethods()
    {
        void Cancel(LeaveRequest l, string r, FederatedUser u) => l.Cancel(r, u, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        void Accept(LeaveRequest l, string r, FederatedUser u) => l.Accept(r, u);
        void Deprecate(LeaveRequest l, string r, FederatedUser u) => l.Deprecate(r, u);
        yield return new object[] { Cancel };
        yield return new object[] { Accept };
        yield return new object[] { Deprecate };
    }

    public static IEnumerable<object[]> GetDoNothingMethodWithTwoRemarksOrAcceptMethodWithThreeRemarksAndFakeRemarksAndEvent(FederatedUser user)
    {
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var fakeRemarks = "fake remarks";
        var fakeRejectRemarks = "fake reject remarks";
        void DoNoting(LeaveRequest leaveRequest, string? remarks, FederatedUser federatedUser)
        { }
        void Accept(LeaveRequest leaveRequest, string? remarks, FederatedUser federatedUser) => leaveRequest.Accept(remarks, federatedUser);
        yield return new object[]
        {
            DoNoting,
            new List<LeaveRequest.RemarksModel>
            {
                new(@event.Remarks!, @event.CreatedBy),
                new(fakeRejectRemarks, user)
            },
            fakeRemarks,
            fakeRejectRemarks,
            @event
        };
        yield return new object[]
        {
            Accept,
            new List<LeaveRequest.RemarksModel>
            {
                new(@event.Remarks!, @event.CreatedBy),
                new(fakeRemarks, user),
                new(fakeRejectRemarks, user)
            },
            fakeRemarks,
            fakeRejectRemarks,
            @event
        };
    }
}
