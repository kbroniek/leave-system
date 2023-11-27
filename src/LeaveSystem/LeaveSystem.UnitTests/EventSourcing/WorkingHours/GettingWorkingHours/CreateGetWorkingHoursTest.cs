using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.GettingWorkingHours;

public class CreateGetWorkingHoursTest
{
    [Fact]
    public void WhenPageNumberLowerThanZero_ThenThrowArgumentException()
    {
        //Given
        var currentDate = DateTimeOffset.Now;
        //When
        var act = () =>
        {
            GetWorkingHours.Create(
                3,
                -1,
                currentDate,
                currentDate + new TimeSpan(4, 0, 0, 0),
                new[] { "1" },
                FakeUserProvider.GetUserWithNameFakeoslav(),
                new[] { WorkingHoursStatus.Current }
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
            GetWorkingHours.Create(
                pageSize,
                5,
                currentDate,
                currentDate + new TimeSpan(4, 0, 0, 0),
                new[] { "1" },
                FakeUserProvider.GetUserWithNameFakeoslav(),
                new[] { WorkingHoursStatus.Current }
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
        var getLeaveRequests = GetWorkingHours.Create(
            null,
            null,
            null,
            null,
            null,
            user,
            null
        );
        //Then
        getLeaveRequests.Should().BeEquivalentTo(new
        {
            PageNumber = 1,
            PageSize = 20,
            DateFrom = utcNow.Add(TimeSpan.FromDays(-14)).GetDayWithoutTime(),
            DateTo = utcNow.Add(TimeSpan.FromDays(14)).GetDayWithoutTime(),
            Statuses = new[] { WorkingHoursStatus.Current },
            CreatedByEmails = (string[]?)null,
            UserIds = (string[]?)null,
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
        var statuses = new[] { WorkingHoursStatus.Current, WorkingHoursStatus.Deprecated };
        var userIds = new[] { "1" };
        //When
        var getLeaveRequests = GetWorkingHours.Create(
            pageSize,
            pageNumber,
            dateFrom,
            dateTo,
            userIds,
            user,
            statuses
        );
        //Then
        getLeaveRequests.Should().BeEquivalentTo(new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Statuses = statuses,
            UserIds = userIds,
            RequestedBy = user,
        });
    }
}