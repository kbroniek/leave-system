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
using LeaveSystem.UnitTests.TestDataGenerators;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests;

public class RejectLeaveRequestTest
{
    private static readonly FederatedUser User = FakeUserProvider.GetUserWithNameFakeoslav();
    
    [Theory]
    [MemberData(nameof(Get_WhenLeaveRequestStatusOtherThenPendingAndAccepted_ThenThrowInvalidOperationException_TestData))]
    public void WhenLeaveRequestStatusOtherThenPendingAndAccepted_ThenThrowInvalidOperationException(Action<LeaveRequest,string, FederatedUser> actionBeforeReject)
    {
        //Given
        var createEvent = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(
            createEvent
        );
        actionBeforeReject(leaveRequest, "fake remarks", User);
        //When
        var act = () =>
        {
            leaveRequest.Reject("fake remarks", User);
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
        leaveRequest.Reject(remarks, User);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
            {
                Status = LeaveRequestStatus.Rejected,
                LastModifiedBy = User,
                Remarks = new[] {new LeaveRequest.RemarksModel(@event.Remarks, @event.CreatedBy)},
            }, o => o.ExcludingMissingMembers()
        );
        leaveRequest.DequeueUncommittedEvents().Should().BeEquivalentTo(
            new IEvent[] {@event, LeaveRequestAccepted.Create(leaveRequest.Id, remarks, User)}
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
        actionBeforeReject(leaveRequest, fakeRemarks, User);
        //When
        leaveRequest.Reject(fakeRejectRemarks, User);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
            {
                Status = LeaveRequestStatus.Rejected,
                LastModifiedBy = User,
                Remarks = fakeRemarksCollection
            }, o => o.ExcludingMissingMembers()
        );
        var dequeuedEvents = leaveRequest.DequeueUncommittedEvents();
        dequeuedEvents.Length.Should().Be(fakeRemarksCollection.Count);
        dequeuedEvents.Last().Should().BeEquivalentTo(new
            {
                LeaveRequestId = leaveRequest.Id,
                Remarks = fakeRejectRemarks,
                RejectedBy = User,
            }, o => o.ExcludingMissingMembers()
        );
    }
    
    public static IEnumerable<object[]>
        Get_WhenStatusIsPendingOrAcceptedAndRemarksIsNotNullOrWhitespace_ThenAddRemarksAndEnqueueEvent_TestData()
    {
        return LeaveRequestTestDataGenerator
            .GetDoNothingMethodWithTwoRemarksOrAcceptMethodWithThreeRemarksAndFakeRemarksAndEvent(User);
    }
}