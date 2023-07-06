using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.Shared;
using Xunit;

namespace LeaveSystem.UnitTests.LeaveRequests.CancelingLeaveRequest;

public class CreateCancelLeaveRequestTest
{
    [Fact]
    public void WhenCancelLeaveRequestPropertiesValid_ThenUserIsCreatedWithSameProperties()
    {
        //Given
        var leaveRequestId = Guid.NewGuid();
        const string remarks = "fake remarks";
        var acceptedBy = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
        //When
        var acceptLeaveRequest = AcceptLeaveRequest.Create(
            leaveRequestId,
            remarks,
            acceptedBy
        );
        //Then
        acceptLeaveRequest.Should().BeEquivalentTo(new
        {
            LeaveRequestId = leaveRequestId,
            Remarks = remarks,
            AcceptedBy = acceptedBy
        }, o => o.ExcludingMissingMembers());
    }
}