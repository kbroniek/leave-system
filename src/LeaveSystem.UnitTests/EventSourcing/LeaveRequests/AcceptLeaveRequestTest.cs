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
                Id = @event.LeaveRequestId,
                DateFrom = @event.DateFrom,
                DateTo = @event.DateTo,
                Duration = @event.Duration,
                LeaveTypeId = @event.LeaveTypeId,
                Status = LeaveRequestStatus.Accepted,
                CreatedBy = @event.CreatedBy,
                LastModifiedBy = user,
                Remarks = new[] {new LeaveRequest.RemarksModel(@event.Remarks, @event.CreatedBy)},
            }, o => o.ExcludingMissingMembers()
        );
        leaveRequest.DequeueUncommittedEvents().Should().BeEquivalentTo(
            new IEvent[] {@event, LeaveRequestAccepted.Create(leaveRequest.Id, remarks, user)}
        );
    }
}