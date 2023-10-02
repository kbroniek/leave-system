using System;
using System.Collections.Generic;
using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Date;
using LeaveSystem.Shared.WorkingHours;
using Marten;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class DateValidatorTest
{
    private LeaveSystemDbContext dbContext;
    private readonly Mock<IDocumentSession> documentSessionMock = new();

    private CreateLeaveRequestValidator GetSut() =>
        new(dbContext, documentSessionMock.Object, new CurrentDateService());

    [Theory]
    [MemberData(nameof(GetDateOutOfYearTestData))]
    public void WhenDateIsOutOfYear_ThenThrowArgumentOutOfRangeException(DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        //Given
        var fakeUser = FederatedUser.Create("1", "good.email@fake.com", "John");
        var @event = LeaveRequestCreated.Create(
            Guid.NewGuid(),
            dateFrom,
            dateTo,
            WorkingHoursUtils.DefaultWorkingHours * 3,
            Guid.NewGuid(),
            "fake remarks",
            fakeUser,
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = GetSut();
        //When
        var act = () => { sut.DateValidator(@event); };
        //Then
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    public static IEnumerable<object[]> GetDateOutOfYearTestData()
    {
        yield return new object[]
        {
            new DateTimeOffset(2022, 12, 31, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
        yield return new object[]
        {
            new DateTimeOffset(2023, 12, 31, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
        yield return new object[]
        {
            new DateTimeOffset(2022, 12, 31, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
    }

    [Fact]
    public void WhenDateFromIsGreaterThanDateTo_ThenThrowArgumentOutOfRangeException()
    {
        //Given
        var fakeUser = FederatedUser.Create("1", "good.email@fake.com", "John");
        var @event = LeaveRequestCreated.Create(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 12, 31, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 10, 1, 0, 0, 0, TimeSpan.Zero),
            WorkingHoursUtils.DefaultWorkingHours * 3,
            Guid.NewGuid(),
            "fake remarks",
            fakeUser,
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = GetSut();
        //When
        var act = () => { sut.DateValidator(@event); };
        //Then
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Date from has to be less than date to.*");
    }
}