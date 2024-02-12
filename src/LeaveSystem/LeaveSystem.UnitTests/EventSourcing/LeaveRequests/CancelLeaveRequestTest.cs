using FluentAssertions;
using GoldenEye.Backend.Core.DDD.Events;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.TestDataGenerators;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests;

public class CancelLeaveRequestTest
{
    private static readonly FederatedUser User = FakeUserProvider.GetUserWithNameFakeoslav();
    [Fact]
    public void WhenCreatedByIdNotEqualsCanceledById_ThenThrowInvalidOperationException()
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        var canceledBy = FederatedUser.Create("2", "second.user@fake.com", "Stanislaw");
        //When
        var act = () => leaveRequest.Cancel("fake remarks", canceledBy, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        //Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Canceling a non-your leave request is not allowed.");
    }

    [Theory]
    [MemberData(nameof(Get_WhenLeaveRequestStatusDifferentThanPendingOrAccepted_thenThrowInvalidOperationException_TestData))]
    public void WhenLeaveRequestStatusDifferentThanPendingOrAccepted_thenThrowInvalidOperationException(Action<LeaveRequest, string, FederatedUser> actionBeforeCancel)
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        var canceledBy = FakeUserProvider.GetUserWithNameFakeoslav();
        actionBeforeCancel(leaveRequest, "fake remarks", @event.CreatedBy);
        //When
        var act = () => leaveRequest.Cancel("fake cancel remarks", canceledBy, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        //Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Canceling leave requests in '{leaveRequest.Status}' status is not allowed.");
    }

    public static IEnumerable<object[]>
        Get_WhenLeaveRequestStatusDifferentThanPendingOrAccepted_thenThrowInvalidOperationException_TestData()
    {
        void Cancel(LeaveRequest l, string? r, FederatedUser u) => l.Cancel(r, u, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        void Reject(LeaveRequest l, string? r, FederatedUser u) => l.Reject(r, u);
        yield return new object[] { Cancel };
        yield return new object[] { Reject };
    }

    [Theory]
    [MemberData(nameof(Get_WhenDateFromIsPastDate_ThenThrowInvalidOperationException_TestData))]
    public void WhenDateFromIsPastDate_ThenThrowInvalidOperationException(TimeSpan timeToSubtract)
    {
        //Given
        var utcNow = DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00");
        var @event = LeaveRequestCreated.Create(
            Guid.NewGuid(),
            utcNow - timeToSubtract,
            utcNow + TimeSpan.FromDays(5),
            TimeSpan.FromHours(16),
            Guid.NewGuid(),
            "fake created remarks",
            User,
            WorkingHoursUtils.DefaultWorkingHours
        );
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        //When
        var act = () => leaveRequest.Cancel("fake cancel remarks", User, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        //Then
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Canceling of past leave requests is not allowed.");
    }

    public static IEnumerable<object[]> Get_WhenDateFromIsPastDate_ThenThrowInvalidOperationException_TestData()
    {
        yield return new object[] { TimeSpan.FromHours(8) };
        yield return new object[] { TimeSpan.FromDays(2) };
        yield return new object[] { TimeSpan.FromMilliseconds(1) };
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void WhenRemarksAreNullOrWhitespace_ThenNotAddRemarks(string? remarks)
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        //When
        leaveRequest.Cancel(remarks, User, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Status = LeaveRequestStatus.Canceled,
            LastModifiedBy = User,
            Remarks = new[] { new LeaveRequest.RemarksModel(@event.Remarks!, @event.CreatedBy) },
        }, o => o.ExcludingMissingMembers());
        (leaveRequest as IEventSource).PendingEvents.Should().BeEquivalentTo(
            new IEvent[] { @event, LeaveRequestAccepted.Create(leaveRequest.Id, remarks, User) }
        );
    }

    [Theory]
    [MemberData(nameof(Get_WhenStatusIsPendingOrAcceptedAndCreatedByIdIsSameAsCanceledByIdAndDateFromIsNotPastDate_ThenEnqueueEventAndAddRemarks_TestData))]
    public void
        WhenStatusIsPendingOrAcceptedAndCreatedByIdIsSameAsCanceledByIdAndDateFromIsNotPastDate_ThenEnqueueEventAndAddRemarks(
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
        leaveRequest.Cancel(fakeRejectRemarks, User, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Status = LeaveRequestStatus.Canceled,
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
        Get_WhenStatusIsPendingOrAcceptedAndCreatedByIdIsSameAsCanceledByIdAndDateFromIsNotPastDate_ThenEnqueueEventAndAddRemarks_TestData()
    {
        return LeaveRequestTestDataGenerator
            .GetDoNothingMethodWithTwoRemarksOrAcceptMethodWithThreeRemarksAndFakeRemarksAndEvent(User);
    }
}
