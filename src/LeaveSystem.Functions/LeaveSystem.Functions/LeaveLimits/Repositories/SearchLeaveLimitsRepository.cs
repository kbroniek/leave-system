namespace LeaveSystem.Functions.LeaveLimits.Repositories;

using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;

public class SearchLeaveLimitsRepository(CosmosClient cosmosClient, string databaseName, string containerId, ILogger<SearchLeaveLimitsRepository> logger)
{
    private const int MaxPageSize = 25;

    public async Task<Result<(IReadOnlyList<LeaveLimitDto> limits, string? continuationToken), Error>> GetLimits(
        int year, string[] assignedToUserIds, Guid[] leaveTypeIds,
        int? pageSize, string? continuationToken, CancellationToken cancellationToken)
    {
        var firstDay = new DateOnly(year, 1, 1);
        var lastDay = new DateOnly(year, 12, 31);
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var pageSizeOrMax = pageSize < MaxPageSize ? pageSize ?? MaxPageSize : MaxPageSize;
        var result = await container.GetItemLinqQueryable<LeaveLimitDto>(continuationToken: continuationToken, requestOptions: new QueryRequestOptions { MaxItemCount = pageSizeOrMax })
            .Where(x => (leaveTypeIds.Length == 0 || leaveTypeIds.Contains(x.LeaveTypeId)) &&
                (assignedToUserIds.Length == 0 || assignedToUserIds.Contains(x.AssignedToUserId)) &&
                x.ValidSince >= firstDay && x.ValidUntil <= lastDay)
            .ToFeedIterator()
            .ExecuteQuery(logger, pageSizeOrMax, cancellationToken);
        return result;
    }

}
