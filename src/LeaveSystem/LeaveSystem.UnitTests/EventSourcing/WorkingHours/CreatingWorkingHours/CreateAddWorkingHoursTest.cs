using System;
using System.Collections.Generic;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.CreatingWorkingHours;

public class CreateAddWorkingHoursTest
{
    [Theory]
    [MemberData(nameof(Get_WhenCreatingWithNullArguments_ThenThrowArgumentNullException_TestData))]
    public void WhenCreatingWithNullArguments_ThenThrowArgumentNullException(
        Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom, TimeSpan? duration, FederatedUser? createdBy)
    {
        //Given
        var dateTo = DateTimeOffsetExtensions.CreateFromDate(2023,2, 4);
        //When
        var act = () =>
        {
            CreateWorkingHours.Create(workingHoursId, userId, dateFrom, dateTo, duration, createdBy);
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }

    public static IEnumerable<object[]> Get_WhenCreatingWithNullArguments_ThenThrowArgumentNullException_TestData()
    {
        var fakeoslav = FakeUserProvider.GetUserWithNameFakeoslav();
        var id = Guid.NewGuid();
        yield return new object[] { null, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), TimeSpan.FromHours(8), fakeoslav};
        yield return new object[] { id, null, DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), TimeSpan.FromHours(8), fakeoslav};
        yield return new object[] { id, "1", null, TimeSpan.FromHours(8), fakeoslav};
        yield return new object[] { id, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), null, fakeoslav};
        yield return new object[] { id, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), TimeSpan.FromHours(8), null};
        yield return new object[] { null, null, null, null, null};
    }
    
    [Theory]
    [MemberData(nameof(Get_WhenCreatingWithDefaultOrWhitespaceArguments_ThenThrowArgumentException_TestData))]
    public void WhenCreatingWithDefaultOrWhitespaceArguments_ThenThrowArgumentException(
        Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom, TimeSpan? duration, FederatedUser? addedBy)
    {
        //Given
        var dateTo = DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6);
        //When
        var act = () =>
        {
            CreateWorkingHours.Create(workingHoursId, userId, dateFrom, dateTo, duration, addedBy);
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> Get_WhenCreatingWithDefaultOrWhitespaceArguments_ThenThrowArgumentException_TestData()
    {
        var fakeoslav = FakeUserProvider.GetUserWithNameFakeoslav();
        var id = Guid.NewGuid();
        yield return new object[] { Guid.Empty, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), TimeSpan.FromHours(8), fakeoslav};
        yield return new object[] { id, "  ", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), TimeSpan.FromHours(8), fakeoslav};
        yield return new object[] { id, "1", new DateTimeOffset(),TimeSpan.FromHours(8), fakeoslav};
        yield return new object[] { id, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), TimeSpan.Zero, fakeoslav};
        yield return new object[] { id, "1", DateTimeOffsetExtensions.CreateFromDate(2022, 2, 4), TimeSpan.FromHours(8), default(FederatedUser)};
        yield return new object[] { Guid.Empty, "", new DateTimeOffset(), TimeSpan.Zero, default(FederatedUser)};
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
        var addedBy = FakeUserProvider.GetUserWithNameFakeoslav();
        //When
        var result = CreateWorkingHours.Create(id, userId, dateFrom, dateTo, duration, addedBy);
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