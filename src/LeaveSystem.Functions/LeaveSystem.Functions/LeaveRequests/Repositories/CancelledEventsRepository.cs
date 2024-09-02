namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Canceling;
using LeaveSystem.Domain.LeaveRequests.Rejecting;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

internal class CancelledEventsRepository(CosmosClient cosmosClient, string databaseName, string containerId)
{
    private readonly IReadOnlyCollection<string> cancelledEventTypes = [
        typeof(LeaveRequestCanceled).AssemblyQualifiedName!,
        typeof(LeaveRequestRejected).AssemblyQualifiedName!
        //TODO: Fill in when you add new events
        //typeof(LeaveRequestDeprecated).AssemblyQualifiedName!
    ];
    public virtual async Task<IReadOnlyCollection<Guid>> GetCanceledStreamIds(List<Guid> streamIds, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var cancelledIterator = container.GetItemLinqQueryable<EventModel<object>>()
                    .Where(x => cancelledEventTypes.Contains(x.EventType) &&
                        streamIds.Contains(x.StreamId))
                    .ToFeedIterator();
        var cancelledEvents = await cancelledIterator.ExecuteQuery(cancellationToken);
        return cancelledEvents.Select(x => x.StreamId).ToList();
    }
}
