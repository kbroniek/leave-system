using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten.Events;
using Marten.Linq;
using Moq;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LeaveSystem.UnitTests.Extensions;

internal static class EventStoreMockExtensions
{
    internal static void SetupLimitValidatorFunctions(this Mock<IEventStore> eventStoreMock,
        IMartenQueryable<LeaveRequestCreated> eventsFromQueryRawEventDataOnly,
        LeaveRequest? leaveRequestFromAggregateStreamAsync)
    {
        eventStoreMock.Setup(v => v.QueryRawEventDataOnly<LeaveRequestCreated>())
            .Returns(eventsFromQueryRawEventDataOnly)
            .Verifiable();
        eventStoreMock.Setup(v => v.AggregateStreamAsync(
                FakeLeaveRequestCreatedProvider.FakeLeaveRequestId,
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<LeaveRequest>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequestFromAggregateStreamAsync)
            .Verifiable();
    }
    internal static IReturnsResult<IEventStore> Setup_QueryRawEventDataOnly<TEvent>(
        this Mock<IEventStore> eventStoreMock,
        IEnumerable<TEvent> eventsFromQueryRawEventDataOnly)
        => eventStoreMock.Setup(v => v.QueryRawEventDataOnly<TEvent>())
            .Returns(new MartenQueryableStub<TEvent>(eventsFromQueryRawEventDataOnly));
    internal static IReturnsResult<IEventStore> Setup_AggregateStreamAsync<TAggregate>(
        this Mock<IEventStore> eventStoreMock,
        TAggregate? leaveRequestFromAggregateStreamAsync,
        Guid streamId) where TAggregate : class
        => eventStoreMock.Setup(v => v.AggregateStreamAsync(
                streamId,
                It.IsAny<long>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<TAggregate>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequestFromAggregateStreamAsync);
}