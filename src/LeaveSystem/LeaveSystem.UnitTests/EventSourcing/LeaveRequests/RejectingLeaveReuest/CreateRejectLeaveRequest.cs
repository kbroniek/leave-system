using System;
using System.Collections.Generic;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.RejectingLeaveReuest;

public class CreateRejectLeaveRequest
{
    [Theory]
    [MemberData(nameof(WhenRejectedByOrEmailOrLeaveRequestIdIsNull_ThenThrowArgumentNullException_TestData))]
    public void WhenRejectedByOrEmailOrLeaveRequestIdIsNull_ThenThrowArgumentNullException(FederatedUser? user, Guid? leaveTypeId)
    {
        //When
        var act = () =>
        {
            RejectLeaveRequest.Create(leaveTypeId, "fake remarks", user);
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    public static IEnumerable<object?[]> WhenRejectedByOrEmailOrLeaveRequestIdIsNull_ThenThrowArgumentNullException_TestData()
    {
        yield return new object?[] { null, Guid.NewGuid() };
        yield return new object?[] { FakeUserProvider.GetUserWithNameFakeoslav(), null };
        yield return new object[] { FederatedUser.Create("1", null, "fake.user"), Guid.NewGuid() };
        yield return new object?[] { null, null };
    }

    [Theory]
    [MemberData(nameof(WhenEmailIsInvalidOrRequestIdIsDefault_ThenThrowArgumentException_TestData))]
    public void WhenEmailIsInvalidOrRequestIdIsDefault_ThenThrowArgumentException(string? fakeEmail, Guid? leaveRequestId)
    {
        //Given
        var user = FederatedUser.Create("1", fakeEmail, "Fakeson");
        //When
        var act = () => RejectLeaveRequest.Create(leaveRequestId, "fake remarks", user);
        //Then
        act.Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> WhenEmailIsInvalidOrRequestIdIsDefault_ThenThrowArgumentException_TestData()
    {
        yield return new object[] { "@user.fake.com", Guid.NewGuid() };
        yield return new object[] { "user.fake.com@", Guid.Empty };
        yield return new object[] { "user@email", Guid.NewGuid() };
        yield return new object[] { "user@email.com", Guid.Empty };
    }
}