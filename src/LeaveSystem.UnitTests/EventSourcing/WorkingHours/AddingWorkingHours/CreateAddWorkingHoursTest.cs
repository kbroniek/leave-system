using System;
using System.Collections.Generic;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;
using LeaveSystem.Shared;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.AddingWorkingHours;

public class CreateAddWorkingHoursTest
{
    [Theory]
    [MemberData(nameof(Get_WhenCreatingWithNullArguments_ThenThrowArgumentNullException_TestData))]
    public void WhenCreatingWithNullArguments_ThenThrowArgumentNullException(
        Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
    {
        //Given
        var duration = TimeSpan.FromHours(8);
        //When
        var act = () =>
        {
            AddWorkingHours.Create(workingHoursId, userId, dateFrom, dateTo, duration);
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    public static IEnumerable<object[]> Get_WhenCreatingWithNullArguments_ThenThrowArgumentNullException_TestData()
    {
        var id = Guid.NewGuid();
        yield return new object[] { null, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6)};
        yield return new object[] { id, null, DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6)};
        yield return new object[] { id, "1", null, DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6)};
        yield return new object[] { id, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), null};
        yield return new object[] { null, null, null, null};
    }
    
    [Theory]
    [MemberData(nameof(Get_WhenCreatingWithDefaultOrWhitespaceArguments_ThenThrowArgumentException_TestData))]
    public void WhenCreatingWithDefaultOrWhitespaceArguments_ThenThrowArgumentException(
        Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
    {
        //Given
        var duration = TimeSpan.FromHours(8);
        //When
        var act = () =>
        {
            AddWorkingHours.Create(workingHoursId, userId, dateFrom, dateTo, duration);
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> Get_WhenCreatingWithDefaultOrWhitespaceArguments_ThenThrowArgumentException_TestData()
    {
        var id = Guid.NewGuid();
        yield return new object[] { Guid.Empty, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6)};
        yield return new object[] { id, "  ", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6)};
        yield return new object[] { id, "1", new DateTimeOffset(), DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6)};
        yield return new object[] { id, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), new DateTimeOffset()};
        yield return new object[] { Guid.Empty, "", new DateTimeOffset(), new DateTimeOffset()};
    }

    [Fact]
    public void WhenAllArgumentsCorrect_ThenCreateWithCorrectProperties()
    {
        //Given
        var id = Guid.NewGuid();
        var userId = "1";
        var dateFrom = DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4);
        var dateTo = DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6);
        var duration = TimeSpan.FromHours(4);
        //When
        var result = AddWorkingHours.Create(id, userId, dateFrom, dateTo, duration);
        //Then
        result.Should().BeEquivalentTo(new
        {
            UserId = userId,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Duration = duration,
            WorkingHoursId = id
        });
    }
}