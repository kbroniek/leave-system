using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.AddingWorkingHours;

public class WorkingHoursCreatedTest
{
    [Fact]
    public void WhenDateFromGreaterThanDateTo_ThenThrowArgumentOutOfRangeException()
    {
        //Given
        //When
        var act = () =>
        {
            WorkingHoursCreated.Create(
                Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2024, 1, 2),
                DateTimeOffsetExtensions.CreateFromDate(2020, 1, 2), TimeSpan.FromHours(8));
        };
        //Then
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WhenArgumentsAreCorrect_ThenCreateObjectWithThisArguments()
    {
        //Given
        var fakeId = Guid.NewGuid();
        var dateFrom = DateTimeOffsetExtensions.CreateFromDate(2020, 1, 2);
        var dateTo = DateTimeOffsetExtensions.CreateFromDate(2024, 1, 2);
        var duration = TimeSpan.FromHours(8);
        //When
        var result =
            WorkingHoursCreated.Create(fakeId, FakeUserProvider.BenId, dateFrom, dateTo, duration);

        //Then
        result.Should().BeEquivalentTo(new
        {
            StreamId = fakeId,
            WorkingHoursId = fakeId,
            UserId = FakeUserProvider.BenId,
            DateFrom = dateFrom, 
            DateTo = dateTo,
            Duration = duration
        });
    }
}