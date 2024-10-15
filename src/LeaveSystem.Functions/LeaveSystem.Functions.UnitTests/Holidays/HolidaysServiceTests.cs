namespace LeaveSystem.Functions.UnitTests.Holidays;
using System;
using System.Collections.Generic;
using System.Linq;
using LeaveSystem.Functions.Holidays;

public class HolidaysServiceTests
{
    private readonly HolidaysService sut = new();

    [Fact]
    public void GetHolidays_ReturnsHolidaysInRange()
    {
        // Arrange
        var dateFrom = new DateOnly(2023, 12, 24);
        var dateTo = new DateOnly(2023, 12, 26);  // Christmas holidays
        var expectedHolidays = new List<DateOnly>
        {
            new DateOnly(2023, 12, 25), // Christmas
            new DateOnly(2023, 12, 26)  // Boxing Day
        };

        // Act
        var result = sut.GetHolidays(dateFrom, dateTo).ToList();

        // Assert
        Assert.Equal(expectedHolidays.Count, result.Count);
        Assert.Equal(expectedHolidays, result);
    }

    [Fact]
    public void GetHolidays_ReturnsEmptyWhenNoHolidaysInRange()
    {
        // Arrange
        var dateFrom = new DateOnly(2023, 12, 1);
        var dateTo = new DateOnly(2023, 12, 3);  // No holidays in this range

        // Act
        var result = sut.GetHolidays(dateFrom, dateTo).ToList();

        // Assert
        Assert.Empty(result);  // No holidays expected in this range
    }

    [Fact]
    public void GetHolidays_SingleDayHoliday()
    {
        // Arrange
        var dateFrom = new DateOnly(2023, 12, 25);
        var dateTo = new DateOnly(2023, 12, 25);  // Single day, Christmas

        var expectedHoliday = new List<DateOnly> { new DateOnly(2023, 12, 25) };

        // Act
        var result = sut.GetHolidays(dateFrom, dateTo).ToList();

        // Assert
        Assert.Single(result);  // Only one holiday expected
        Assert.Equal(expectedHoliday, result);
    }

    [Fact]
    public void GetHolidays_NoHolidayOnSingleWorkDay()
    {
        // Arrange
        var dateFrom = new DateOnly(2023, 12, 22);
        var dateTo = new DateOnly(2023, 12, 22);  // Single workday

        // Act
        var result = sut.GetHolidays(dateFrom, dateTo).ToList();

        // Assert
        Assert.Empty(result);  // No holidays on this workday
    }
}
