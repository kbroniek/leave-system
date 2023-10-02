using System;
using System.Collections.Generic;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequestCreatedTest
{
    private static readonly TimeSpan WorkingHours = TimeSpan.FromHours(8);

[Fact]
    public void WhenEmailIsInvalid_ThenThrowArgumentException()
    {
        //Given
        var fakeUser = FederatedUser.Create("1", "@wrong.email@fakecom.", "John");
        //When
        var act = () =>
        {
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 27, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2023, 7, 30, 0, 0, 0, TimeSpan.Zero),
                WorkingHours * 3,
                Guid.NewGuid(),
                "fake remarks",
                fakeUser,
                WorkingHoursUtils.DefaultWorkingHours
                );
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }
    //Todo: Finish tests for this Method
    [Fact]
    public void WhenEmailIsNull_ThenThrowArgumentNullException()
    {
        //Given
        var fakeUser = FederatedUser.Create("1", null, "John");
        //When
        var act = () =>
        {
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 27, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2023, 7, 30, 0, 0, 0, TimeSpan.Zero),
                WorkingHours * 3,
                Guid.NewGuid(),
                "fake remarks",
                fakeUser,
                WorkingHoursUtils.DefaultWorkingHours
                );
        };
        //Then
        act.Should().Throw<ArgumentNullException>();
    }
    // [Theory]
    // [MemberData(nameof(GetDateOutOfYearTestData))]
    // public void WhenDateIsOutOfYear_ThenThrowArgumentOutOfRangeException(DateTimeOffset dateFrom, DateTimeOffset dateTo)
    // {
    //     //Given
    //     var fakeUser = FederatedUser.Create("1", "good.email@fake.com", "John");
    //     //When
    //     var act = () =>
    //     {
    //         LeaveRequestCreated.Create(
    //             Guid.NewGuid(),
    //             dateFrom,
    //             dateTo,
    //             WorkingHours * 3,
    //             Guid.NewGuid(),
    //             "fake remarks",
    //             fakeUser,
    //             WorkingHoursUtils.DefaultWorkingHours
    //             );
    //     };
    //     //Then
    //     act.Should().Throw<ArgumentOutOfRangeException>();
    // }
    //
    // public static IEnumerable<object[]> GetDateOutOfYearTestData()
    // {
    //     yield return new object[]
    //     {
    //         new DateTimeOffset(2022, 12, 31, 0, 0, 0, TimeSpan.Zero),
    //         new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
    //     };
    //     yield return new object[]
    //     {
    //         new DateTimeOffset(2023, 12, 31, 0, 0, 0, TimeSpan.Zero),
    //         new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
    //     };
    //     yield return new object[]
    //     {
    //         new DateTimeOffset(2022, 12, 31, 0, 0, 0, TimeSpan.Zero),
    //         new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
    //     };
    // }
    // [Fact]
    // public void WhenDateFromIsGreaterThanDateTo_ThenThrowArgumentOutOfRangeException()
    // {
    //     //Given
    //     var fakeUser = FederatedUser.Create("1", "good.email@fake.com", "John");
    //     //When
    //     var act = () =>
    //     {
    //         LeaveRequestCreated.Create(
    //             Guid.NewGuid(),
    //             new DateTimeOffset(2023, 12, 31, 0, 0, 0, TimeSpan.Zero),
    //             new DateTimeOffset(2023, 10, 1, 0, 0, 0, TimeSpan.Zero),
    //             WorkingHours * 3,
    //             Guid.NewGuid(),
    //             "fake remarks",
    //             fakeUser,
    //             WorkingHoursUtils.DefaultWorkingHours
    //             );
    //     };
    //     //Then
    //     act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Date from has to be less than date to.*");
    // }
}