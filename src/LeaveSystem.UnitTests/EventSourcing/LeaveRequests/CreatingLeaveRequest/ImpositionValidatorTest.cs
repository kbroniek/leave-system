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
    private const string FakeLeaveRequestId = "84e9635a-a241-42bb-b304-78d08138b24f";
    private readonly Mock<WorkingHoursService> workingHoursServiceMock = new ();
    private readonly Mock<IDocumentSession> documentSessionMock = new ();
    private readonly Mock<IEventStore> eventStoreMock = new ();
    private readonly FederatedUser fakeUser = FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav");
    private readonly LeaveRequestCreated fakeLeaveRequestCreatedEvent;

    private readonly LeaveRequest fakeLeaveRequestEntity;
    private LeaveSystemDbContext dbContext;

    public ImpositionValidatorTest()
    {
        fakeLeaveRequestCreatedEvent = LeaveRequestCreated.Create(
            Guid.Parse(FakeLeaveRequestId),
            new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
            new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
            TimeSpan.FromDays(6),
            Guid.NewGuid(),
            "fake remarks",
            fakeUser
        );
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
    public async Task WhenThereIsOtherValidLeveRequestWithSameId_ThenThrowValidationException()
    {
        //Given
        var events = new MartenQueryableStub<LeaveRequestCreated>() {
            fakeLeaveRequestCreatedEvent,
        };
        SetupEventStoreMock(events, fakeLeaveRequestEntity);


        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    // System Under Test
    private CreateLeaveRequestValidator GetSut(LeaveSystemDbContext dbContext) =>
        new(dbContext, workingHoursServiceMock.Object, documentSessionMock.Object);
    
    private void SetupEventStoreMock(IMartenQueryable<LeaveRequestCreated> eventsFromQueryRawEventDataOnly, LeaveRequest? leaveRequestFromAggregateStreamAsync)
    {
        eventStoreMock.Setup(v => v.QueryRawEventDataOnly<LeaveRequestCreated>())
            .Returns(eventsFromQueryRawEventDataOnly);
        eventStoreMock.Setup(v => v.AggregateStreamAsync(
                Guid.Parse(FakeLeaveRequestId),
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<LeaveRequest>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequestFromAggregateStreamAsync)
            .Verifiable();
    }

    [Fact]
    public async Task
        WhenThereIsNotOtherLeveRequestWithSameId_ThenNotThrowValidationException()
    {
        //Given
        var events = GetLeaveRequestCreatedEventsWithDifferentIds();
        SetupEventStoreMock(events, fakeLeaveRequestEntity);
        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task
        WhenOtherLeaveRequestWithSameIdIsNotValid_ThenNotThrowValidationException()
    {
        //Given
        var fakeCanceledLeaveRequestEntity = fakeLeaveRequestEntity;
        fakeCanceledLeaveRequestEntity.Cancel("cancel fake remarks",fakeUser);
        var events = GetLeaveRequestCreatedEventsWithDifferentIds();
        events.Add(fakeLeaveRequestCreatedEvent);
        SetupEventStoreMock(events, fakeLeaveRequestEntity);
        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().NotThrowAsync<ValidationException>();
    }

    private MartenQueryableStub<LeaveRequestCreated> GetLeaveRequestCreatedEventsWithDifferentIds()
    {
        return new MartenQueryableStub<LeaveRequestCreated>
        {
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 11, 0, 0, 0, TimeSpan.FromHours(5)),
                new DateTimeOffset(2023, 7, 13, 0, 0, 0, TimeSpan.FromHours(5)),
                TimeSpan.FromDays(6),
                Guid.NewGuid(),
                "fake remarks",
                FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav")
            ),
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
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
