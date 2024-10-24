namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

internal class UsedLeavesRepository(CosmosClient cosmosClient, string databaseName, string containerId, CancelledEventsRepository cancelledEventsRepository) : IUsedLeavesRepository
{
    public async ValueTask<TimeSpan> GetUsedLeavesDuration(
        Guid leaveRequestId, DateOnly? limitValidSince, DateOnly? limitValidUntil,
        string userId, Guid leaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var pendingEvents = await GetPendingEvents(leaveRequestId, limitValidSince, limitValidUntil,
            userId, leaveTypeId, nestedLeaveTypeIds, container, cancellationToken);
        if (pendingEvents.Count == 0)
        {
            return TimeSpan.Zero;
        }
        var pendingEventStreamIds = pendingEvents.Select(x => x.StreamId).ToList();
        //TODO: Check if it works correctly.
        var cancelledEventsStreamIds = await cancelledEventsRepository.GetCanceledStreamIds(pendingEventStreamIds, cancellationToken);

        var countableEvents = pendingEvents.Where(x => !cancelledEventsStreamIds.Contains(x.StreamId));
        return TimeSpan.FromTicks(countableEvents.Sum(x => x.Body.Duration.Ticks));
    }

    private static async Task<IReadOnlyList<EventModel<PendingEventEntity>>> GetPendingEvents(
        Guid leaveRequestId, DateOnly? limitValidSince, DateOnly? limitValidUntil,
        string userId, Guid leaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds,
        Container container, CancellationToken cancellationToken)
    {
        var iterator = container.GetItemLinqQueryable<EventModel<PendingEventEntity>>()
            .Where(x => x.StreamId != leaveRequestId &&
                (x.Body.LeaveTypeId == leaveTypeId || nestedLeaveTypeIds.Contains(x.Body.LeaveTypeId)) &&
                x.Body.AssignedTo.Id == userId &&
                (limitValidSince == null || x.Body.DateFrom >= limitValidSince) &&
                (limitValidUntil == null || x.Body.DateTo <= limitValidUntil) &&
                x.EventType == typeof(LeaveRequestCreated).AssemblyQualifiedName!)
            .ToFeedIterator();

        var pendingEvents = await iterator.ExecuteQuery(cancellationToken);
        return pendingEvents;
    }

    private sealed record PendingEventEntity(Guid LeaveTypeId, EventUserEntity AssignedTo, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration);
    private sealed record EventUserEntity(string Id);
}
