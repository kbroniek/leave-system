using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.Shared;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CancellingLeaveRequests;

public class CancelLeaveRequestTest
{
    [Fact]
    public void WhenAcceptLeaveRequestPropertiesValid_ThenIsCreatedWithSameProperties()
    {
        //Given
        var leaveRequestId = Guid.NewGuid();
        const string remarks = "fake remarks";
        var acceptedBy = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
        //When
        var acceptLeaveRequest = CancelLeaveRequest.Create(
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

    [Fact]
    public void WhenFederatedUserIsNull_ThenThrowArgumentNullException()
    {
        //Given
        var leaveRequestId = Guid.NewGuid();
        const string remarks = "fake remarks";
        //When
        var act = () =>
        {
            CancelLeaveRequest.Create(
                leaveRequestId,
                remarks,
                null
            );
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("fakeUser@.com")]
    [InlineData("@fake.com")]
    [InlineData("fake@user")]
    public void WhenEmailIsInvalid_ThenThrowArgumentException(string email)
    {
        //Given
        var leaveRequestId = Guid.NewGuid();
        const string remarks = "fake remarks";
        var acceptedBy = FederatedUser.Create("1", email, "Fakeoslav");
        //When
        var act = () =>
        {
            CancelLeaveRequest.Create(
                leaveRequestId,
                remarks,
                acceptedBy
            );
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenLeaveRequestIdDefault_ThenThrowArgumentException()
    {
        //Given
        var leaveRequestId = Guid.Empty;
        const string remarks = "fake remarks";
        var acceptedBy = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
        //When
        var act = () =>
        {
            CancelLeaveRequest.Create(
                leaveRequestId,
                remarks,
                acceptedBy
            );
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }
    
    [Fact]
    public void WhenLeaveRequestIdNull_ThenThrowArgumentNullException()
    {
        //Given
        const string remarks = "fake remarks";
        var acceptedBy = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
        //When
        var act = () =>
        {
            CancelLeaveRequest.Create(
                null,
                remarks,
                acceptedBy
            );
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }
}