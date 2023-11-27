using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.DeprecatingLeaveRequest;
using LeaveSystem.Shared;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.DeprecatingLeaveRequest;

public class CreateDeprecateLeaveRequestTest
{
        [Fact]
    public void WhenAcceptLeaveRequestPropertiesValid_ThenIsCreatedWithSameProperties()
    {
        //Given
        var leaveRequestId = Guid.NewGuid();
        const string remarks = "fake remarks";
        var deprecatedBy = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
        //When
        var deprecateLeaveRequest = DeprecateLeaveRequest.Create(
            leaveRequestId,
            remarks,
            deprecatedBy
        );
        //Then
        deprecateLeaveRequest.Should().BeEquivalentTo(new
        {
            LeaveRequestId = leaveRequestId,
            Remarks = remarks,
            AcceptedBy = deprecatedBy
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
            DeprecateLeaveRequest.Create(
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
        var deprecatedBy = FederatedUser.Create("1", email, "Fakeoslav");
        //When
        var act = () =>
        {
            DeprecateLeaveRequest.Create(
                leaveRequestId,
                remarks,
                deprecatedBy
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
        var deprecatedBy = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
        //When
        var act = () =>
        {
            DeprecateLeaveRequest.Create(
                leaveRequestId,
                remarks,
                deprecatedBy
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
        var deprecatedBy = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
        //When
        var act = () =>
        {
            DeprecateLeaveRequest.Create(
                null,
                remarks,
                deprecatedBy
            );
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }
}