namespace LeaveSystem.Functions.LeaveLimits.Repositories;

using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveLimits;
using LeaveSystem.Domain.LeaveRequests.Canceling;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Rejecting;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

public class LeaveRequestEventsRepository(
    CosmosClient cosmosClient,
    string databaseName,
    string containerId,
    ILogger<LeaveRequestEventsRepository> logger)
{
    public async Task<Result<IReadOnlyList<LeaveRequestEventDto>, Error>> GetPendingEventsForYear(
        int year,
        CancellationToken cancellationToken)
    {
        var firstDay = new DateOnly(year, 1, 1);
        var lastDay = new DateOnly(year, 12, 31);
        var container = cosmosClient.GetContainer(databaseName, containerId);

        var result = await container.GetItemLinqQueryable<EventModel<PendingEventEntity>>()
            .Where(x =>
                x.EventType == typeof(LeaveRequestCreated).AssemblyQualifiedName! &&
                x.Body.DateFrom >= firstDay &&
                x.Body.DateTo <= lastDay)
            .ToFeedIterator()
            .ExecuteQuery(logger, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error;
        }

        var events = result.Value.Select(e => new LeaveRequestEventDto(
            StreamId: e.StreamId,
            AssignedToUserId: e.Body.AssignedTo.Id,
            LeaveTypeId: e.Body.LeaveTypeId,
            DateFrom: e.Body.DateFrom,
            DateTo: e.Body.DateTo,
            Duration: e.Body.Duration))
            .ToList();

        return events;
    }

    public async Task<Result<IReadOnlyCollection<Guid>, Error>> GetCancelledStreamIds(
        IEnumerable<Guid> streamIds,
        CancellationToken cancellationToken)
    {
        var streamIdsList = streamIds.ToList();
        if (streamIdsList.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        var container = cosmosClient.GetContainer(databaseName, containerId);
        var cancelledEventTypes = new[]
        {
            typeof(LeaveRequestCanceled).AssemblyQualifiedName!,
            typeof(LeaveRequestRejected).AssemblyQualifiedName!
        };

        var cancelledIterator = container.GetItemLinqQueryable<EventModel<object>>()
            .Where(x =>
                cancelledEventTypes.Contains(x.EventType) &&
                streamIdsList.Contains(x.StreamId))
            .ToFeedIterator();

        var cancelledEvents = await cancelledIterator.ExecuteQuery(logger, cancellationToken);

        if (cancelledEvents.IsFailure)
        {
            return cancelledEvents.Error;
        }

        return cancelledEvents.Value.Select(x => x.StreamId).Distinct().ToList();
    }

    private sealed record PendingEventEntity(
        Guid LeaveTypeId,
        EventUserEntity AssignedTo,
        DateOnly DateFrom,
        DateOnly DateTo,
        TimeSpan Duration);

    private sealed record EventUserEntity(string Id);
}
