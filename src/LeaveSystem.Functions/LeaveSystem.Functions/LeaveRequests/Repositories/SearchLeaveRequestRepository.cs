namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Searching;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using static LeaveSystem.Domain.LeaveRequests.Searching.ISearchLeaveRequestRepository;

internal class SearchLeaveRequestRepository(CosmosClient cosmosClient, string databaseName, string containerId, ILogger<SearchLeaveRequestRepository> logger) : ISearchLeaveRequestRepository
{
    private const int PageSize = 25;

    public async Task<(IEnumerable<PendingEventEntity> pendingEvents, string? continuationToken)> GetPendingEvents(
        string? continuationToken, DateOnly dateFrom, DateOnly dateTo,
        Guid[] leaveTypeIds, string[] assignedToUserIds, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var iterator = container.GetItemLinqQueryable<EventModel<PendingEventEntity>>(continuationToken: continuationToken, requestOptions: new QueryRequestOptions { MaxItemCount = PageSize })
            .Where(x => (leaveTypeIds.Length == 0 || leaveTypeIds.Contains(x.Body.LeaveTypeId)) &&
                (assignedToUserIds.Length == 0 || assignedToUserIds.Contains(x.Body.AssignedTo.Id)) && (
                (x.Body.DateFrom <= dateTo && x.Body.DateTo >= dateTo) ||
                (x.Body.DateFrom <= dateFrom && x.Body.DateTo >= dateFrom) ||
                (x.Body.DateFrom >= dateFrom && x.Body.DateTo <= dateTo) ||
                (x.Body.DateFrom <= dateFrom && x.Body.DateTo >= dateTo)) &&
                x.EventType == typeof(LeaveRequestCreated).AssemblyQualifiedName!)
            .ToFeedIterator();

        (var pendingEvents, var continuationTokenResult) = await iterator.ExecuteQuery(PageSize, cancellationToken);
        return (pendingEvents.Select(x => x.Body), continuationTokenResult);
    }
}
