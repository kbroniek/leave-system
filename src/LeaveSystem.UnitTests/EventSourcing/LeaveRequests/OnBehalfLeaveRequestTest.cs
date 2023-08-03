using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using GoldenEye.Events;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
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

    [Fact]
    public void WhenLeaveRequestStatusIsPending_ThenEnqueueAndApplyEvent()
    {
        //Given
        var createEvent = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(
            createEvent
        );
        //When
        leaveRequest.OnBehalf(User);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
            {
                CreatedByOnBehalf = User,
                LastModifiedBy = User
            }, o => o.ExcludingMissingMembers()
        );
        leaveRequest.DequeueUncommittedEvents().Should().BeEquivalentTo(
            new IEvent[] { createEvent, LeaveRequestOnBehalfCreated.Create(leaveRequest.Id, User)});
    }

    public static IEnumerable<object[]>
        Get_WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException_TestData()
    {
        return LeaveRequestTestDataGenerator.GetCancelAcceptAndRejectMethods();
    }
}