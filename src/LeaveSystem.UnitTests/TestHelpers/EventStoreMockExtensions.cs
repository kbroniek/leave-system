using System;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.UnitTests.Providers;
using Marten.Events;
using Marten.Linq;
using Moq;

namespace LeaveSystem.UnitTests.TestHelpers;

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
}