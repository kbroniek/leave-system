using System;
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

public class RejectLeaveRequestTest
{
    private static readonly FederatedUser user = FakeUserProvider.GetUserWithNameFakeoslav();
    
    [Theory]
    [MemberData(nameof(Get_WhenLeaveRequestStatusOtherThenPendingAndAccepted_ThenThrowInvalidOperationException_TestData))]
    public void WhenLeaveRequestStatusOtherThenPendingAndAccepted_ThenThrowInvalidOperationException(Action<LeaveRequest,string, FederatedUser> actionBeforeReject)
    {
        //Given
        var createEvent = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(
            createEvent
        );
        actionBeforeReject(leaveRequest, "fake remarks", user);
        //When
        var act = () =>
        {
            leaveRequest.Reject("fake remarks", user);
        };
        //Then
        act.Should().Throw<InvalidOperationException>();
        leaveRequest.DequeueUncommittedEvents().Length.Should().Be(2);
    }

    public static IEnumerable<object[]>
        Get_WhenLeaveRequestStatusOtherThenPendingAndAccepted_ThenThrowInvalidOperationException_TestData()
    {
        void Cancel(LeaveRequest l, string? r, FederatedUser u) => l.Cancel(r, u);
        void Reject(LeaveRequest l, string? r, FederatedUser u) => l.Reject(r, u);
        yield return new object[] { Cancel };
        yield return new object[] { Reject };
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void WhenCreatingWithNullOrWhitespaceRemarks_ThenCreateLeaveRequestWithoutRemarks(string? remarks)
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        //When
        leaveRequest.Reject(remarks, user);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
            {
                Status = LeaveRequestStatus.Rejected,
                LastModifiedBy = user,
                Remarks = new[] {new LeaveRequest.RemarksModel(@event.Remarks, @event.CreatedBy)},
            }, o => o.ExcludingMissingMembers()
        );
        leaveRequest.DequeueUncommittedEvents().Should().BeEquivalentTo(
            new IEvent[] {@event, LeaveRequestAccepted.Create(leaveRequest.Id, remarks, user)}
        );
    }

    [Theory]
    [MemberData(nameof(Get_WhenStatusIsPendingOrAcceptedAndRemarksIsNotNullOrWhitespace_ThenAddRemarksAndEnqueueEvent_TestData))]
    public void WhenStatusIsPendingOrAcceptedAndRemarksIsNotNullOrWhitespace_ThenAddRemarksAndEnqueueEvent(
        Action<LeaveRequest,string?, FederatedUser> actionBeforeReject,
        List<LeaveRequest.RemarksModel> fakeRemarksCollection,
        string? fakeRemarks,
        string? fakeRejectRemarks,
        LeaveRequestCreated @event
        )
    {
        //Given
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        actionBeforeReject(leaveRequest, fakeRemarks, user);
        //When
        leaveRequest.Reject(fakeRejectRemarks, user);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
            {
                Status = LeaveRequestStatus.Rejected,
                LastModifiedBy = user,
                Remarks = fakeRemarksCollection
            }, o => o.ExcludingMissingMembers()
        );
        var dequeuedEvents = leaveRequest.DequeueUncommittedEvents();
        dequeuedEvents.Length.Should().Be(fakeRemarksCollection.Count());
        dequeuedEvents.Last().Should().BeEquivalentTo(new
            {
                LeaveRequestId = leaveRequest.Id,
                Remarks = fakeRejectRemarks,
                RejectedBy = user,
            }, o => o.ExcludingMissingMembers()
        );
    }
    
    public static IEnumerable<object[]>
        Get_WhenStatusIsPendingOrAcceptedAndRemarksIsNotNullOrWhitespace_ThenAddRemarksAndEnqueueEvent_TestData()
    {
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var fakeRemarks = "fake remarks";
        var fakeRejectRemarks = "fake reject remarks";
        void DoNoting(LeaveRequest leaveRequest, string? remarks, FederatedUser federatedUser) {}
        void Accept(LeaveRequest leaveRequest, string? remarks, FederatedUser federatedUser) => leaveRequest.Accept(remarks, federatedUser);
        yield return new object[]
        {
            DoNoting, 
            new List<LeaveRequest.RemarksModel>
            {
                new(@event.Remarks, @event.CreatedBy),
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
                new(@event.Remarks, @event.CreatedBy),
                new(fakeRemarks, user),
                new(fakeRejectRemarks, user)
            },
            fakeRemarks,
            fakeRejectRemarks,
            @event
        };
    }
}