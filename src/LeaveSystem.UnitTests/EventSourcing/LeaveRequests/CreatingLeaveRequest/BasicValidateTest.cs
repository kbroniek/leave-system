using System;
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

    public async Task InitializeAsync()
    {
        dbContext = await DbContextFactory.CreateDbContextAsync();
        requestValidator = new CreateLeaveRequestValidator(dbContext, workingHoursServiceMock.Object, documentSessionMock.Object);
    }
    
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public void WhenDurationOutOfRange_ThrowOutOfRangeException()
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Empty, 
            DateTimeOffset.UtcNow, 
            DateTimeOffset.UtcNow, 
            TimeSpan.FromHours(8),
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
}