using System;
using System.Threading;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using Marten;
using Moq;

namespace LeaveSystem.UnitTests.Extensions;

internal static class DocumentSessionMockExtensions
{
    internal static void VerifyLeaveRequestValidatorFunctions(
        this Mock<IDocumentSession> documentSessionMock, Guid leaveRequestId, Times queryRawEventDataOnlyTimes,
        Times aggregateStreamAsyncTimes)
    {
        documentSessionMock.Verify(x => 
            x.Events.QueryRawEventDataOnly<LeaveRequestCreated>(), queryRawEventDataOnlyTimes);
        documentSessionMock.Verify(x => 
            x.Events.AggregateStreamAsync(leaveRequestId, It.IsAny<long>(), It.IsAny<DateTimeOffset?>(), 
                It.IsAny<LeaveRequest>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), aggregateStreamAsyncTimes);
    }
}