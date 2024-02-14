using GoldenEye.Backend.Core.DDD.Events;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests;

public class CreatePendingLeaveRequestTest
{
    [Fact]
    public void WhenEventProvided_ThenCreateLeaveRequestWithThoseProperties()
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetSickLeaveRequest();
        //When
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Id = @event.LeaveRequestId,
            @event.DateFrom,
            @event.DateTo,
            @event.Duration,
            @event.LeaveTypeId,
            Status = LeaveRequestStatus.Pending,
            @event.CreatedBy,
            LastModifiedBy = @event.CreatedBy,
            Remarks = new[] { new LeaveRequest.RemarksModel(@event.Remarks!, @event.CreatedBy) }
        }, o => o.ExcludingMissingMembers());
        (leaveRequest as IEventSource).PendingEvents.Should().BeEquivalentTo(
            new[] { @event }
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void WhenCreatingWithNullOrWhitespaceRemarks_ThenCreateLeaveRequestWithoutRemarks(string? remarks)
    {
        //Given
        var now = DateTimeOffset.Now;
        var @event = LeaveRequestCreated.Create(
            Guid.NewGuid(),
            new DateTimeOffset(now.Year, 2, 7, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(now.Year, 2, 10, 0, 0, 0, TimeSpan.Zero),
            TimeSpan.FromHours(16),
            Guid.NewGuid(),
            remarks,
            FakeUserProvider.GetUserWithNameFakeoslav(),
            WorkingHoursUtils.DefaultWorkingHours
            );
        //When
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Id = @event.LeaveRequestId,
            @event.DateFrom,
            @event.DateTo,
            @event.Duration,
            @event.LeaveTypeId,
            Status = LeaveRequestStatus.Pending,
            @event.CreatedBy,
            LastModifiedBy = @event.CreatedBy,
            Remarks = Enumerable.Empty<LeaveRequest.RemarksModel>()
        }, o => o.ExcludingMissingMembers()
        );
        (leaveRequest as IEventSource).PendingEvents.Should().BeEquivalentTo(
            new[] { @event }
        );
    }
}
