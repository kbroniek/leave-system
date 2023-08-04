using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using Marten;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class BasicValidateTest : IAsyncLifetime
{
    private CreateLeaveRequestValidator requestValidator;
    private LeaveSystemDbContext dbContext;
    private readonly Mock<WorkingHoursService> workingHoursServiceMock = new();
    private readonly Mock<IDocumentSession> documentSessionMock = new();
    public static IEnumerable<object?[]> GetDateTestData()
    {
        yield return new object?[]
        {
            new DateTimeOffset(2023, 7, 9, 0, 0, 0, TimeSpan.FromHours(5)),     //Not working day
            new DateTimeOffset(2023, 7, 10, 0, 0, 0, TimeSpan.FromHours(5)),     //Working day
            TimeSpan.FromHours(8)
        };
        yield return new object?[]
        {
            new DateTimeOffset(2023, 7, 7, 0, 0, 0, TimeSpan.FromHours(5)),     //Working day
            new DateTimeOffset(2023, 7, 8, 0, 0, 0, TimeSpan.FromHours(5)),      //Not working day
            TimeSpan.FromHours(8)
        };
        yield return new object?[]
        {
            new DateTimeOffset(2023, 7, 8, 0, 0, 0, TimeSpan.FromHours(5)),     //Not working day
            new DateTimeOffset(2023, 7, 9, 0, 0, 0, TimeSpan.FromHours(5)),      //Not working day
            TimeSpan.FromHours(8)
        };
        yield return new object?[]
        {
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),     //Working day
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),      //Working day
            TimeSpan.FromDays(4)
        };
    }
    public async Task InitializeAsync()
    {
        dbContext = await DbContextFactory.CreateDbContextAsync();
        requestValidator = new CreateLeaveRequestValidator(dbContext, workingHoursServiceMock.Object, documentSessionMock.Object);
    }
    
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Theory]
    [MemberData(nameof(GetDateTestData))]
    public void WhenDateFromDateToOrDurationIsWorkingDay_ThenThrowArgumentOutOfRangeException(
        DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan duration
    )
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Empty, 
            dateFrom, 
            dateTo, 
            duration,
            Guid.NewGuid(),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        );
        //When
        var act = () =>
        {
            requestValidator.BasicValidate(
                leaveRequestCreated,
                TimeSpan.FromHours(10),
                TimeSpan.FromHours(11),
                false);
        };
        //Then
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
    
    [Fact]
    public void WhenDataCorrect_NotArgumentOutOfRangeException()
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Empty, 
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),   
            TimeSpan.FromDays(6),
            Guid.NewGuid(),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
        );
        //When
        var act = () =>
        {
            requestValidator.BasicValidate(
                leaveRequestCreated,
                TimeSpan.FromDays(5),
                TimeSpan.FromDays(10),
                false);
        };
        //Then
        act.Should().NotThrow<ArgumentOutOfRangeException>();
    }
}