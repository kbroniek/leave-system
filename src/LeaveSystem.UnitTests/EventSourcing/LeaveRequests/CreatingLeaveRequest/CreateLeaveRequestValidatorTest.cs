using System;
using System.Threading.Tasks;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using Marten;
using Marten.Events;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public abstract class CreateLeaveRequestValidatorTest : IAsyncLifetime
{
    protected const string FakeLeaveRequestId = "84e9635a-a241-42bb-b304-78d08138b24f";
    protected readonly Mock<WorkingHoursService> WorkingHoursServiceMock = new ();
    protected readonly Mock<IDocumentSession> DocumentSessionMock = new ();
    protected readonly Mock<IEventStore> EventStoreMock = new ();
    protected readonly FederatedUser FakeUser = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
    protected readonly LeaveRequestCreated FakeLeaveRequestCreatedEvent;
    protected const string FakeHolidayLeaveGuid = "d0db6559-43e8-4916-a93c-f87a3c75afe8";

    protected readonly LeaveRequest FakeLeaveRequestEntity;
    protected  LeaveSystemDbContext DbContext;
    
    protected CreateLeaveRequestValidatorTest()
    {
        var currentDate = DateTimeOffset.UtcNow;
        var dateFrom = DateCalculator.GetNextWorkingDay(currentDate + new TimeSpan(2, 0, 0, 0));
        var twoDaysAfterDateFrom = dateFrom + new TimeSpan(2, 0, 0, 0);
        var dateTo = DateCalculator.GetNextWorkingDay(twoDaysAfterDateFrom);
        FakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            Guid.Parse(FakeLeaveRequestId),
            dateFrom,
            dateTo,
            TimeSpan.FromDays(6),
            Guid.Parse(FakeHolidayLeaveGuid),
            "fake remarks",
            FakeUser
        );
        FakeLeaveRequestEntity = LeaveRequest.CreatePendingLeaveRequest(FakeLeaveRequestCreatedEvent);
        DocumentSessionMock.SetupGet(v => v.Events)
            .Returns(EventStoreMock.Object);
    }

    public virtual async Task InitializeAsync()
    {
        DbContext = await DbContextFactory.CreateDbContextAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
    
    
    // System Under Test
    protected CreateLeaveRequestValidator GetSut(LeaveSystemDbContext dbContext) =>
        new(dbContext, WorkingHoursServiceMock.Object, DocumentSessionMock.Object);
}