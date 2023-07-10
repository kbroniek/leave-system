using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.TestHelpers;
using Marten;
using Marten.Events;
using Marten.Linq;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class ImpositionValidatorTest : IAsyncLifetime
{
    private const string fakeLeaveRequestId = "84e9635a-a241-42bb-b304-78d08138b24f";
    private readonly Mock<WorkingHoursService> workingHoursServiceMock = new ();
    private readonly Mock<IDocumentSession> documentSessionMock = new ();
    private readonly Mock<IEventStore> eventStoreMock = new ();
    private readonly LeaveRequestCreated fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
        Guid.Parse(fakeLeaveRequestId),
        new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
        new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
        TimeSpan.FromDays(6),
        Guid.NewGuid(),
        "fake remarks",
        FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
    );

    private readonly LeaveRequest fakeLeaveRequestEntity;
    private LeaveSystemDbContext dbContext;

    public ImpositionValidatorTest()
    {
        fakeLeaveRequestEntity = LeaveRequest.CreatePendingLeaveRequest(fakeLeaveRequestCreatedEvent);
        documentSessionMock.SetupGet(v => v.Events)
            .Returns(eventStoreMock.Object);
    }

    public async Task InitializeAsync()
    {
        dbContext = await DbContextFactory.CreateDbContextAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task WhenOtherLeveRequestOverlappingWithThisDate_ThenThrowValidationException()
    {
        //Given
        var events = new MartenQueryableStub<LeaveRequestCreated>() {
            fakeLeaveRequestCreatedEvent
        };
        eventStoreMock.Setup(v => v.QueryRawEventDataOnly<LeaveRequestCreated>())
            .Returns(events);
        eventStoreMock.Setup(v => v.AggregateStreamAsync(
                Guid.Parse(fakeLeaveRequestId),
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<LeaveRequest>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeLeaveRequestEntity)
            .Verifiable();

        //var leaveRequestCreatedEvents = GetLeaveRequestCreatedEvents().AsQueryable();
        //var leaveRequestsFromDb = GetLeaveRequestsFromDb();
        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    // System Under Test
    private CreateLeaveRequestValidator GetSut(LeaveSystemDbContext dbContext) =>
        new CreateLeaveRequestValidator(dbContext, workingHoursServiceMock.Object, documentSessionMock.Object);

    private IEnumerable<LeaveRequest> GetLeaveRequestsFromDb()
    {
        return new[]
        {
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            CreateLeaveRequestFromParams(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            )
        };
    }

    private LeaveRequest CreateLeaveRequestFromParams(
        Guid leaveRequestId,
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        TimeSpan duration,
        Guid leaveTypeId,
        string? remarks,
        FederatedUser createdBy)
    {
        var @event =
            LeaveRequestCreated.Create(leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, remarks, createdBy);
        return LeaveRequest.CreatePendingLeaveRequest(@event);
    }

    private List<LeaveRequestCreated> GetLeaveRequestCreatedEvents()
    {
        return new List<LeaveRequestCreated>()
        {
            LeaveRequestCreated.Create(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            LeaveRequestCreated.Create(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            LeaveRequestCreated.Create(
                Guid.Empty,
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
        };
    }
}
