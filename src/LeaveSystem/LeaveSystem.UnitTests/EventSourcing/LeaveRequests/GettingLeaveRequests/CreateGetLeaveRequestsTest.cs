using System;
using System.Collections.Generic;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.GettingLeaveRequests;

public class CreateGetLeaveRequestsTest
{
    [Fact]
    public void WhenPageNumberLowerThanZero_ThenThrowArgumentException()
    {
        //Given
        var currentDate = DateTimeOffset.Now;
        //When
        var act = () =>
        {
            GetLeaveRequests.Create(
                -1,
                3,
                currentDate,
                currentDate + new TimeSpan(4, 0, 0, 0),
                new[] { Guid.NewGuid() },
                new[] { LeaveRequestStatus.Accepted },
                new[] { "fake@email.com" },
                new[] { "1" },
                FakeUserProvider.GetUserWithNameFakeoslav()
            );
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1001)]
    public void WhenPageSizeOutOfRange_ThenThrowOutOfRangeException(int pageSize)
    {
        //Given
        var currentDate = DateTimeOffset.Now;
        //When
        var act = () =>
        {
            GetLeaveRequests.Create(
                3,
                pageSize,
                currentDate,
                currentDate + new TimeSpan(4, 0, 0, 0),
                new[] { Guid.NewGuid() },
                new[] { LeaveRequestStatus.Accepted },
                new[] { "fake@email.com" },
                new[] { "1" },
                FakeUserProvider.GetUserWithNameFakeoslav()
            );
        };
        //Then
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WhenAllNullableArgumentsAreNull_ThenCreateObjectWithDefaultNotNullableProperties()
    {
        //Given
        var utcNow = DateTimeOffset.UtcNow;
        var user = FakeUserProvider.GetUserWithNameFakeoslav();
        //When
        var getLeaveRequests = GetLeaveRequests.Create(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                user
            );
            //Then
        getLeaveRequests.Should().BeEquivalentTo(new
        {
            PageNumber = 1,
            PageSize = 20,
            DateFrom = utcNow.Add(TimeSpan.FromDays(-14)).GetDayWithoutTime(),
            DateTo = utcNow.Add(TimeSpan.FromDays(14)).GetDayWithoutTime(),
            LeaveTypeIds = (Guid[]?)null,
            Statuses = new[] { LeaveRequestStatus.Accepted, LeaveRequestStatus.Pending },
            CreatedByEmails = (string[]?)null,
            CreatedByUserIds = (string[]?)null,
            RequestedBy = user,
        }, o => o.ExcludingMissingMembers());
    }

    [Fact]
    public void WhenAllArgumentsAreNotNullValidValues_ThenCreateObjectWithThoseProperties()
    {
        //Given
        var currentDate = DateTimeOffset.Now;
        var user = FakeUserProvider.GetUserWithNameFakeoslav();
        var pageNumber = 2;
        var pageSize = 3;
        var dateFrom = currentDate.GetDayWithoutTime();
        var dateTo = (currentDate + new TimeSpan(4, 0, 0, 0)).GetDayWithoutTime();
        var leaveTypeIds = new[] { Guid.NewGuid() };
        var statuses = new[] { LeaveRequestStatus.Accepted };
        var createdByEmail = new[] { "fake@email.com" };
        var createdByUserIds = new[] { "1" };
        //When
        var getLeaveRequests = GetLeaveRequests.Create(
            pageNumber,
            pageSize,
            dateFrom,
            dateTo,
            leaveTypeIds,
            statuses,
            createdByEmail,
            createdByUserIds,
            user
            );
        //Then
        getLeaveRequests.Should().BeEquivalentTo(new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            DateFrom = dateFrom,
            DateTo = dateTo,
            LeaveTypeIds = leaveTypeIds,
            Statuses = statuses,
            CreatedByEmails = createdByEmail,
            CreatedByUserIds = createdByUserIds,
            RequestedBy = user,
        });
    }
}