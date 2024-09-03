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
        var pendingEvents = await container.GetItemLinqQueryable<EventModel<PendingEventEntity>>()
            .Where(x => x.Body.AssignedTo.Id == createdById && (
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
            .ToFeedIterator()
            .ExecuteQuery(cancellationToken);
        if (pendingEvents.Count == 0)
        {
            return false;
        }
        var pendingEventStreamIds = pendingEvents.Select(x => x.StreamId).ToList();
        //TODO: Check if it works correctly.
        var cancelledEventsStreamIds = await cancelledEventsRepository.GetCanceledStreamIds(pendingEventStreamIds, cancellationToken);
        //TODO: Need to fix because we need to check if all streams are canceled.
        return cancelledEventsStreamIds.Count == 0;
    }
    private sealed record PendingEventEntity(Guid LeaveTypeId, EventUserEntity AssignedTo, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration);
    private sealed record EventUserEntity(string Id);
}
