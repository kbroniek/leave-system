using System;
using System.Collections.Generic;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateCreateLeaveRequestTest
{
    public static IEnumerable<object?[]> GetNullTestData()
    {
        yield return new object?[]
        {
            null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, Guid.NewGuid(),
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        };
        yield return new object?[]
        {
            Guid.NewGuid(), null, DateTimeOffset.UtcNow, Guid.NewGuid(),
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        };
        yield return new object?[]
        {
            Guid.NewGuid(), DateTimeOffset.UtcNow, null, Guid.NewGuid(),
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        };
        yield return new object?[]
        {
            Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null,
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        };
        yield return new object?[]
        {
            Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, Guid.NewGuid(), null
        };
        yield return new object?[] { null, null, null, null, null };
    }

    public static IEnumerable<object?[]> GetDefaultTestData()
    {
        yield return new object?[]
        {
            Guid.Empty, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, Guid.NewGuid(),
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        };
        yield return new object?[]
        {
            Guid.NewGuid(), default(DateTimeOffset), DateTimeOffset.UtcNow, Guid.NewGuid(),
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        };
        yield return new object?[]
        {
            Guid.NewGuid(), DateTimeOffset.UtcNow, default(DateTimeOffset), Guid.NewGuid(),
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        };
        yield return new object?[]
        {
            Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, Guid.Empty,
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        };
        yield return new object?[]
        {
            Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, Guid.NewGuid(), default(FederatedUser)
        };
        yield return new object?[]
            { Guid.Empty, default(DateTimeOffset), default(DateTimeOffset), Guid.Empty, default(FederatedUser) };
    }

    [Theory]
    [MemberData(nameof(GetNullTestData))]
    public void WhenOneOfCreateLeaveRequestPropertiesIsNull_ThenThrowArgumentNullException(
        Guid? leaveRequestId,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        Guid? leaveTypeId,
        FederatedUser? createdBy)
    {
        //Given
        TimeSpan? duration = null;
        const string remarks = "fake remarks";
        //When
        var act = () =>
        {
            CreateLeaveRequest.Create(
                leaveRequestId,
                dateFrom,
                dateTo,
                duration,
                leaveTypeId,
                remarks,
                createdBy
            );
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(GetDefaultTestData))]
    public void WhenOneOfCreateLeaveRequestPropertiesIsDefault_ThenThrowArgumentException(
        Guid? leaveRequestId,
        DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        Guid? leaveTypeId,
        FederatedUser? createdBy)
    {
        //Given
        TimeSpan? duration = null;
        const string remarks = "fake remarks";
        //When
        var act = () =>
        {
            CreateLeaveRequest.Create(
                leaveRequestId,
                dateFrom,
                dateTo,
                duration,
                leaveTypeId,
                remarks,
                createdBy
            );
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenAllPropertiesAreNotNullOrDefault_CreateCreateLeaveRequest()
    {
        //Given
        var leaveRequestId = Guid.NewGuid();
        var dateFrom = DateTimeOffset.UtcNow;
        var dateTo = DateTimeOffset.MaxValue;
        var duration = TimeSpan.MaxValue;
        var leaveTypeId = Guid.NewGuid();
        const string remarks = "fake remarks";
        var createdBy = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
        //When
        var createLeaveRequest = CreateLeaveRequest.Create(
            leaveRequestId,
            dateFrom,
            dateTo,
            duration,
            leaveTypeId,
            remarks,
            createdBy
        );
        //Then
        createLeaveRequest.Should().BeEquivalentTo(new
        {
            LeaveRequestId = leaveRequestId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Duration = duration,
            LeaveTypeId = leaveTypeId,
            Remarks = remarks,
            CreatedBy = createdBy
        }, o => o.ExcludingMissingMembers());
    }
}