namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

internal class ImpositionValidatorRepository(
    CosmosClient cosmosClient, string databaseName, string containerId,
    CancelledEventsRepository cancelledEventsRepository) : IImpositionValidatorRepository
{
    public async ValueTask<bool> IsExistValid(
        Guid leaveRequestId, string createdById,
        DateOnly dateFrom, DateOnly dateTo,
        CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var pendingEvents = await container.GetItemLinqQueryable<EventModel<PendingEventEntity>>()
            .Where(x => x.StreamId != leaveRequestId && x.Body.AssignedTo.Id == createdById &&
                x.EventType == typeof(LeaveRequestCreated).AssemblyQualifiedName! && (
                (x.Body.DateFrom <= dateTo && x.Body.DateTo >= dateTo) ||
                (x.Body.DateFrom <= dateFrom && x.Body.DateTo >= dateFrom) ||
                (x.Body.DateFrom >= dateFrom && x.Body.DateTo <= dateTo) ||
                (x.Body.DateFrom <= dateFrom && x.Body.DateTo >= dateTo)
            ))
            .ToFeedIterator()
            .ExecuteQuery(cancellationToken);
        if (pendingEvents.Count == 0)
        {
            return false;
        }
        var pendingEventStreamIds = pendingEvents.Select(x => x.StreamId).ToList();
        var cancelledEventsStreamIds = await cancelledEventsRepository.GetCanceledStreamIds(pendingEventStreamIds, cancellationToken);
        return !pendingEventStreamIds.TrueForAll(cancelledEventsStreamIds.Contains);
    }
    private sealed record PendingEventEntity(Guid LeaveTypeId, EventUserEntity AssignedTo, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration);
    private sealed record EventUserEntity(string Id);
}
