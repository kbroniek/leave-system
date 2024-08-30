namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

internal class ImpositionValidatorRepository(CosmosClient cosmosClient, string databaseName, string containerId, CancelledEventsRepository cancelledEventsRepository) : IImpositionValidatorRepository
{
    public async ValueTask<bool> IsExistValid(string createdById, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var iterator = container.GetItemLinqQueryable<EventModel<PendingEventEntity>>()
            .Where(x => x.Body.AssignedTo.UserId == createdById && (
                (x.Body.DateFrom >= dateTo &&
                x.Body.DateTo <= dateTo)
             ||
                (x.Body.DateFrom >= dateFrom &&
                x.Body.DateTo <= dateFrom)
             ||
                (x.Body.DateFrom >= dateFrom &&
                x.Body.DateTo <= dateTo)
             ||
                (x.Body.DateFrom <= dateFrom &&
                x.Body.DateTo >= dateTo)
            ))
            .ToFeedIterator();
        var pendingEvents = await iterator.ExecuteQuery(cancellationToken);
        if (pendingEvents.Count == 0)
        {
            return false;
        }
        var pendingEventStreamIds = pendingEvents.Select(x => x.StreamId).ToList();
        //TODO: Check if it works correctly.
        var cancelledEventsStreamIds = await cancelledEventsRepository.GetCancelledStreamIds(pendingEventStreamIds, cancellationToken);
        return cancelledEventsStreamIds.Count == 0;
    }
    private sealed record PendingEventEntity(Guid LeaveTypeId, EventUserEntity AssignedTo, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration);
    private sealed record EventUserEntity(string UserId);
}
