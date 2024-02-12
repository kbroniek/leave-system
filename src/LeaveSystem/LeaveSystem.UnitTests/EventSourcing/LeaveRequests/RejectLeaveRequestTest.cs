using FluentAssertions;
using GoldenEye.Backend.Core.DDD.Events;
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
    public void WhenLeaveRequestStatusOtherThenPendingAndAccepted_ThenThrowInvalidOperationException(Action<LeaveRequest, string, FederatedUser> actionBeforeReject)
    {
        //Given
        var createEvent = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(createEvent);
        actionBeforeReject(leaveRequest, "fake remarks", User);
        //When
        var act = () => leaveRequest.Reject("fake remarks", User);
        //Then
        act.Should().Throw<InvalidOperationException>();
        (leaveRequest as IEventSource).PendingEvents.Count.Should().Be(2);
    }

    public static IEnumerable<object[]>
        Get_WhenLeaveRequestStatusOtherThenPendingAndAccepted_ThenThrowInvalidOperationException_TestData()
    {
        void Cancel(LeaveRequest l, string? r, FederatedUser u) => l.Cancel(r, u, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        void Reject(LeaveRequest l, string? r, FederatedUser u) => l.Reject(r, u);
        void Deprecate(LeaveRequest l, string? r, FederatedUser u) => l.Deprecate(r, u);
        yield return new object[] { Cancel };
        yield return new object[] { Reject };
        yield return new object[] { Deprecate };
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
            Remarks = new[] { new LeaveRequest.RemarksModel(@event.Remarks!, @event.CreatedBy) },
        }, o => o.ExcludingMissingMembers()
        );
        (leaveRequest as IEventSource).PendingEvents.Should().BeEquivalentTo(
            new IEvent[] { @event, LeaveRequestAccepted.Create(leaveRequest.Id, remarks, User) }
        );
    }

    [Fact]
    public void WhenStatusIsAccepted_ThenShouldReject()
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var fakeRejectRemarks = "fake reject remarks";
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        leaveRequest.Accept(null, User);
        //When
        leaveRequest.Reject(fakeRejectRemarks, User);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Status = LeaveRequestStatus.Rejected,
            LastModifiedBy = User,
        }, o => o.ExcludingMissingMembers());
    }

    [Theory]
    [MemberData(nameof(Get_WhenStatusIsPendingOrAcceptedAndRemarksIsNotNullOrWhitespace_ThenAddRemarksAndEnqueueEvent_TestData))]
    public void WhenStatusIsPendingOrAcceptedAndRemarksIsNotNullOrWhitespace_ThenAddRemarksAndEnqueueEvent(
        Action<LeaveRequest, string?, FederatedUser> actionBeforeReject,
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
        var dequeuedEvents = (leaveRequest as IEventSource).PendingEvents;
        dequeuedEvents.Count.Should().Be(fakeRemarksCollection.Count);
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
