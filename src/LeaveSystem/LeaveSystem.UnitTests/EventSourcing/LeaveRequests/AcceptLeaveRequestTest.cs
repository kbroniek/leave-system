using FluentAssertions;
using GoldenEye.Backend.Core.DDD.Events;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.TestDataGenerators;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests;

public class AcceptLeaveRequestTest
{
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly FederatedUser user = FakeUserProvider.GetUserWithNameFakeoslav();

    [Theory]
    [MemberData(nameof(Get_WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException_TestData))]
    public void WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException(Action<LeaveRequest, string, FederatedUser> actionBeforeAccept)
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
        leaveRequest.PendingEvents.Count.Should().Be(2);
    }

    public static IEnumerable<object[]>
        Get_WhenLeaveRequestStatusOtherThanPending_ThenThrowInvalidOperationException_TestData()
    {
        return LeaveRequestTestDataGenerator.GetCancelAcceptAndRejectMethods();
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
            Remarks = new[] { new LeaveRequest.RemarksModel(@event.Remarks!, @event.CreatedBy) },
        }, o => o.ExcludingMissingMembers());
        leaveRequest.PendingEvents.Should().BeEquivalentTo(
            new IEvent[] { @event, LeaveRequestAccepted.Create(leaveRequest.Id, remarks, user) }
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
        leaveRequest.Accept(fakeRejectRemarks, user);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Status = LeaveRequestStatus.Accepted,
            LastModifiedBy = user,
            Remarks = new[]
                {
                    new LeaveRequest.RemarksModel(@event.Remarks!, @event.CreatedBy),
                    new LeaveRequest.RemarksModel(fakeRejectRemarks, user)
                }
        }, o => o.ExcludingMissingMembers());
        var dequeuedEvents = leaveRequest.PendingEvents;
        dequeuedEvents.Count.Should().Be(2);
        dequeuedEvents.Last().Should().BeEquivalentTo(new
        {
            LeaveRequestId = leaveRequest.Id,
            Remarks = fakeRejectRemarks,
            RejectedBy = user,
        }, o => o.ExcludingMissingMembers());
    }

    [Fact]
    public void WhenStatusIsRejected_ThenShouldAccept()
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var fakeRejectRemarks = "fake reject remarks";
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        leaveRequest.Reject(null, user);
        //When
        leaveRequest.Accept(fakeRejectRemarks, user);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Status = LeaveRequestStatus.Accepted,
            LastModifiedBy = user,
        }, o => o.ExcludingMissingMembers());
    }
}
