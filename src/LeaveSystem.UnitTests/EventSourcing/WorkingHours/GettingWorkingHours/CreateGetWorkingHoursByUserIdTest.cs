using System;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.GettingWorkingHours;

public class CreateGetWorkingHoursByUserIdTest
{
    [Fact]
    public void WhenUserIdIsNull_ThenThrowArgumentNullException()
    {
        //Given
        var userId = (string?)null;
        //When
        var act = () => { GetCurrentWorkingHoursByUserId.Create(userId); };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("   ")]
    [InlineData(" ")]
    public void WhenUserIdIsWhiteSpace_ThenThrowArgumentException(string userId)
    {
        //Given
        //When
        var act = () => { GetCurrentWorkingHoursByUserId.Create(userId); };
        //Then
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenArgumentsAreCorrect_ThenCreateObject()
    {
        //Given
        var userId = "1234";
        //When
        var result = GetCurrentWorkingHoursByUserId.Create(userId);
        //Then
        result.Should().BeEquivalentTo(new
        {
            UserId = userId
        });
    }
}