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

public class ImpositionValidatorTest : CreateLeaveRequestValidatorTest, IAsyncLifetime
{
    [Fact]
    public async Task WhenThereIsOtherValidLeveRequestWithSameId_ThenThrowValidationException()
    {
        //Given
        var events = new MartenQueryableStub<LeaveRequestCreated>() {
            FakeLeaveRequestCreatedEvent,
        };
        SetupEventStoreMock(events, FakeLeaveRequestEntity);


        var sut = GetSut(DbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(FakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().ThrowAsync<ValidationException>();
    }

    private void SetupEventStoreMock(IMartenQueryable<LeaveRequestCreated> eventsFromQueryRawEventDataOnly, LeaveRequest? leaveRequestFromAggregateStreamAsync)
    {
        EventStoreMock.Setup(v => v.QueryRawEventDataOnly<LeaveRequestCreated>())
            .Returns(eventsFromQueryRawEventDataOnly);
        EventStoreMock.Setup(v => v.AggregateStreamAsync(
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
        SetupEventStoreMock(events, FakeLeaveRequestEntity);
        var sut = GetSut(DbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(FakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task
        WhenOtherLeaveRequestWithSameIdIsNotValid_ThenNotThrowValidationException()
    {
        //Given
        var fakeCanceledLeaveRequestEntity = FakeLeaveRequestEntity;
        fakeCanceledLeaveRequestEntity.Cancel("cancel fake remarks",FakeUser);
        var events = GetLeaveRequestCreatedEventsWithDifferentIds();
        events.Add(FakeLeaveRequestCreatedEvent);
        SetupEventStoreMock(events, FakeLeaveRequestEntity);
        var sut = GetSut(DbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(FakeLeaveRequestCreatedEvent); };
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
