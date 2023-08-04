using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequestDetails;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.GettingLeaveRequestDetails;

public class CreateGetLeaveRequestDetailsTest
{
    [Fact]
    public void WhenLeaveRequestIdIsNull_ThenThrowArgumentNullException()
    {
        //Given
        Guid? leaveRequestId = null;
        //When
        var act = () => { GetLeaveRequestDetails.Create(leaveRequestId); };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenAllPropertiesValid_CreateGetLeaveRequestDetails()
    {
        //Given
        Guid? leaveRequestId = new Guid();
        //When
        var getLeaveRequestDetails = GetLeaveRequestDetails.Create(leaveRequestId);
        //Then
        getLeaveRequestDetails.Should().BeEquivalentTo(new
        {
            LeaveRequestId = leaveRequestId
        });
    }
}