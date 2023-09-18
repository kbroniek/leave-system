using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.ModyfingWorkingHours;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.ModifyingWorkingHours;

public class WorkingHoursModifiedTest
{
    [Fact]
    public void WhenDateFromGreaterThanDateTo_ThenThrowArgumentOutOfRangeException()
    {
        //Given
        //When
        var act = () =>
        {
            WorkingHoursModified.Create(
                Guid.NewGuid(), DateTimeOffsetExtensions.CreateFromDate(2024, 1, 2),
                DateTimeOffsetExtensions.CreateFromDate(2020, 1, 2), TimeSpan.FromHours(8), FakeUserProvider.GetUserWithNameFakeoslav());
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
        var fakeAdmin = FakeUserProvider.GetUserWithNameFakeoslav();
        //When
        var result =
            WorkingHoursModified.Create(fakeId, dateFrom, dateTo, duration, fakeAdmin);

        //Then
        result.Should().BeEquivalentTo(new
        {
            StreamId = fakeId,
            WorkingHoursId = fakeId,
            DateFrom = dateFrom, 
            DateTo = dateTo,
            Duration = duration,
            ModifiedBy = fakeAdmin
        });
    }
}