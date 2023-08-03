using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.TestDataGenerators;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests;

public class OnBehalfLeaveRequestTest
{
    private static readonly FederatedUser User = FakeUserProvider.GetUserWithNameFakeoslav();
    
    [Theory]
    [MemberData(nameof(Get_WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException_TestData))]
    public void WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException(Action<LeaveRequest,string, FederatedUser> actionBeforeAccept)
    {
        //Given
        var createEvent = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(
            createEvent
        );
        actionBeforeAccept(leaveRequest, "fake remarks", User);
        //When
        var act = () =>
        {
            leaveRequest.OnBehalf(User);
        };
        //Then
        act.Should().Throw<InvalidOperationException>();
        leaveRequest.DequeueUncommittedEvents().Length.Should().Be(2);
    }

    public static IEnumerable<object[]>
        Get_WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException_TestData()
    {
        return LeaveRequestTestDataGenerator.GetCancelAcceptAndRejectMethods();
    }
}