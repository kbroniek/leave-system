using System;
using System.Linq;
using FluentAssertions;
using GoldenEye.Events;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests;

public class CreatePendingLeaveRequestTest
{
    [Fact]
    public void WhenEventProvided_ThenCreateLeaveRequestWithThoseProperties()
    {
        //Given
        var @event = FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedWithSickLeave();
        //When
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(@event);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
            {
                Id = @event.LeaveRequestId,
                DateFrom = @event.DateFrom,
                DateTo = @event.DateTo,
                Duration = @event.Duration,
                LeaveTypeId = @event.LeaveTypeId,
                Status = LeaveRequestStatus.Pending,
                CreatedBy = @event.CreatedBy,
                LastModifiedBy = @event.CreatedBy,
                Remarks = new[] {new LeaveRequest.RemarksModel(@event.Remarks, @event.CreatedBy)}
            }, o => o.ExcludingMissingMembers()
        );
        leaveRequest.DequeueUncommittedEvents().Should().BeEquivalentTo(
            new [] {@event}
        );
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void WhenCreatingWithNullOrWhitespaceRemarks_ThenCreateLeaveRequestWithoutRemarks(string remarks)
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
                DateFrom = @event.DateFrom,
                DateTo = @event.DateTo,
                Duration = @event.Duration,
                LeaveTypeId = @event.LeaveTypeId,
                Status = LeaveRequestStatus.Pending,
                CreatedBy = @event.CreatedBy,
                LastModifiedBy = @event.CreatedBy,
                Remarks = Enumerable.Empty<LeaveRequest.RemarksModel>()
            }, o => o.ExcludingMissingMembers()
        );
        leaveRequest.DequeueUncommittedEvents().Should().BeEquivalentTo(
            new [] {@event}
        );
    }
}