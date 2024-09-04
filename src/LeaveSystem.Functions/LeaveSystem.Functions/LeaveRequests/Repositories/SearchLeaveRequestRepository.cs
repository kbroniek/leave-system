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
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

internal class SearchLeaveRequestRepository(CosmosClient cosmosClient, string databaseName, string containerId) : ISearchLeaveRequestRepository
{
    private const int MaxSize = 25;

    public async Task<(IEnumerable<SearchLeaveRequestsResultDto> results, string? continuationToken)> Search(
        string? continuationToken, DateOnly dateFrom, DateOnly dateTo,
        Guid[]? leaveTypeIds, LeaveRequestStatus[] statuses, string[]? assignedToUserIds, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var iterator = container.GetItemLinqQueryable<EventModel<PendingEventEntity>>(continuationToken: continuationToken)
            .Where(x => (leaveTypeIds == null || leaveTypeIds.Contains(x.Body.LeaveTypeId)) &&
                (assignedToUserIds == null || assignedToUserIds.Contains(x.Body.AssignedTo.Id)) && (
                (x.Body.DateFrom <= dateTo && x.Body.DateTo >= dateTo) ||
                (x.Body.DateFrom <= dateFrom && x.Body.DateTo >= dateFrom) ||
                (x.Body.DateFrom >= dateFrom && x.Body.DateTo <= dateTo) ||
                (x.Body.DateFrom <= dateFrom && x.Body.DateTo >= dateTo)) &&
                x.EventType == typeof(LeaveRequestCreated).AssemblyQualifiedName!)
            .ToFeedIterator();

        var pendingEvents = await iterator.ExecuteQuery(MaxSize, cancellationToken);
        //TODO: Get all streams and check statuses
        return (null, pendingEvents.continuationToken);
    }
    private sealed record PendingEventEntity(Guid LeaveTypeId, EventUserEntity AssignedTo, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration);
    private sealed record EventUserEntity(string Id);
}
