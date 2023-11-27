using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.RejectingLeaveReuest;

public class CreateLeaveRequestRejected
{
    [Fact]
    public void WhenValuesProvided_ThenCreateObject()
    {
        //Given
        var leaveRequestId = Guid.NewGuid();
        var remarks = "fake remarks";
        var user = FakeUserProvider.GetUserWithNameFakeoslav();
        //When
        var @event = LeaveRequestRejected.Create(
            leaveRequestId,
            remarks,
            user
        );
        //Then
        @event.Should().BeEquivalentTo(new
        {
            RejectedBy = user,
            Remarks = remarks,
            LeaveRequestId = leaveRequestId
        });
    }
}