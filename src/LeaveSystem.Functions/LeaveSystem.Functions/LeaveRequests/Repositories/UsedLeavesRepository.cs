namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

internal class UsedLeavesRepository(CosmosClient cosmosClient, string databaseName, string containerId, TimeProvider timeProvider) : IUsedLeavesRepository
{
    //TODO: Fill in when you add new events
    private readonly IReadOnlyCollection<string> cancelledEventTypes = [
        //typeof(LeaveRequestCancelled).AssemblyQualifiedName!
        //typeof(LeaveRequestRejected).AssemblyQualifiedName!
        //typeof(LeaveRequestDeprecated).AssemblyQualifiedName!
        ];
    public async ValueTask<TimeSpan> GetUsedLeavesDuration(DateOnly dateFrom, DateOnly dateTo, string userId, Guid leaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var pendingEvents = await GetPendingEvents(userId, leaveTypeId, nestedLeaveTypeIds, container, cancellationToken);
        if (pendingEvents.Count == 0)
        {
            return TimeSpan.Zero;
        }
        var pendingEventStreamIds = pendingEvents.Select(x => x.StreamId).ToList();
        //TODO: Check if it works correctly.
        var cancelledEventsStreamIds = await GetCancelledStreamIds(container, pendingEventStreamIds, cancellationToken);

        var countableEvents = pendingEvents.Where(x => !cancelledEventsStreamIds.Contains(x.StreamId));
        return TimeSpan.FromTicks(countableEvents.Sum(x => x.Body.Duration.Ticks));
    }

    private async Task<IReadOnlyCollection<Guid>> GetCancelledStreamIds(Container container, List<Guid> pendingEventStreamIds, CancellationToken cancellationToken)
    {
        var cancelledIterator = container.GetItemLinqQueryable<EventModel<object>>()
                    .Where(x => cancelledEventTypes.Contains(x.EventType) &&
                        pendingEventStreamIds.Contains(x.StreamId))
                    .ToFeedIterator();
        var cancelledEvents = await cancelledIterator.ExecuteQuery(cancellationToken);
        return cancelledEvents.Select(x => x.StreamId).ToList();
    }

    private async Task<IReadOnlyList<EventModel<PendingEventEntity>>> GetPendingEvents(string userId, Guid leaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds, Container container, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var dateOnlyNow = DateOnly.FromDateTime(now.Date);
        var firstDayOfYear = dateOnlyNow.GetFirstDayOfYear();
        var lastDayOfYear = dateOnlyNow.GetLastDayOfYear();
        var iterator = container.GetItemLinqQueryable<EventModel<PendingEventEntity>>()
            .Where(x => (x.Body.LeaveTypeId == leaveTypeId || nestedLeaveTypeIds.Contains(x.Body.LeaveTypeId)) &&
                x.Body.AssignedTo.UserId == userId &&
                x.Body.DateFrom >= firstDayOfYear &&
                x.Body.DateTo <= lastDayOfYear &&
                x.EventType == typeof(LeaveRequestCreated).AssemblyQualifiedName!)
            .ToFeedIterator();

        var pendingEvents = await iterator.ExecuteQuery(cancellationToken);
        return pendingEvents;
    }

    private sealed record PendingEventEntity(Guid LeaveTypeId, EventUserEntity AssignedTo, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration);
    private sealed record EventUserEntity(string UserId);
}
