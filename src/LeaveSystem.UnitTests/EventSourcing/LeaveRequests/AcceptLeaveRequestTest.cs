using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GoldenEye.Events;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests;

public class AcceptLeaveRequestTest
{
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly FederatedUser user = FakeUserProvider.GetUserWithNameFakeoslav();
    
    [Theory]
    [MemberData(nameof(Get_WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException_TestData))]
    public void WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException(Action<LeaveRequest,string, FederatedUser> actionBeforeAccept)
    {
        //Given
        var createEvent = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(
            createEvent
        );
        actionBeforeAccept(leaveRequest, "fake remarks", user);
        //When
        var act = () =>
        {
            leaveRequest.Accept("fake remarks", user);
        };
        //Then
        act.Should().Throw<InvalidOperationException>();
        leaveRequest.DequeueUncommittedEvents().Length.Should().Be(2);
    }

    public static IEnumerable<object[]>
        Get_WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException_TestData()
    {
        void Cancel(LeaveRequest l, string r, FederatedUser u) => l.Cancel(r, u);
        void Accept(LeaveRequest l, string r, FederatedUser u) => l.Accept(r, u);
        void Reject(LeaveRequest l, string r, FederatedUser u) => l.Reject(r, u);
        yield return new object[] { Cancel };
        yield return new object[] { Accept };
        yield return new object[] { Reject };
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void WhenCreatingWithNullOrWhitespaceRemarks_ThenCreateLeaveRequestWithoutRemarks(string remarks)
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        //When
        leaveRequest.Accept(remarks, user);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
            {
                Status = LeaveRequestStatus.Accepted,
                LastModifiedBy = user,
                Remarks = new[] {new LeaveRequest.RemarksModel(@event.Remarks, @event.CreatedBy)},
            }, o => o.ExcludingMissingMembers()
        );
        leaveRequest.DequeueUncommittedEvents().Should().BeEquivalentTo(
            new IEvent[] {@event, LeaveRequestAccepted.Create(leaveRequest.Id, remarks, user)}
        );
    }

    [Fact]
    public void WhenStatusIsPendingAndRemarksIsNotNullOrWhitespace_ThenAddRemarksAndEnqueueEvent()
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var fakeRejectRemarks = "fake reject remarks";
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        //When
        leaveRequest.Reject(fakeRejectRemarks, user);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
            {
                Status = LeaveRequestStatus.Rejected,
                LastModifiedBy = user,
                Remarks = new []
                {
                    new LeaveRequest.RemarksModel(@event.Remarks, @event.CreatedBy),
                    new LeaveRequest.RemarksModel(fakeRejectRemarks, user)
                }
            }, o => o.ExcludingMissingMembers()
        );
        var dequeuedEvents = leaveRequest.DequeueUncommittedEvents();
        dequeuedEvents.Length.Should().Be(2);
        dequeuedEvents.Last().Should().BeEquivalentTo(new
            {
                LeaveRequestId = leaveRequest.Id,
                Remarks = fakeRejectRemarks,
                RejectedBy = user,
            }, o => o.ExcludingMissingMembers()
        );
    }
}