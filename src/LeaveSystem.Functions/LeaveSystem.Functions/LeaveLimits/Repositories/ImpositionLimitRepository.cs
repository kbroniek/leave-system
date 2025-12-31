namespace LeaveSystem.Functions.LeaveLimits.Repositories;

using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using static LeaveSystem.Shared.Dto.LeaveLimitDto;

public class ImpositionLimitRepository(CosmosClient cosmosClient, string databaseName, string containerId, ILogger<ImpositionLimitRepository> logger)
{
    public async Task<Result<(IReadOnlyList<LeaveLimitEntity> limits, string? continuationToken), Error>> GetLimits(
        DateOnly? validSince, DateOnly? validUntil, string? assignedToUserId, Guid leaveTypeId, Guid? leaveLimitId,
        int? pageSize, string? continuationToken, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var pageSizeOrMax = pageSize.PageSizeOrMax();
        var result = await container.GetItemLinqQueryable<LeaveLimitEntity>(continuationToken: continuationToken, requestOptions: new QueryRequestOptions { MaxItemCount = pageSizeOrMax })
            .Where(x =>
                x.State == LeaveLimitState.Active &&
                leaveTypeId == x.LeaveTypeId &&
                (leaveLimitId == null || x.Id != leaveLimitId) && (
                (assignedToUserId != null && assignedToUserId == x.AssignedToUserId) ||
                (assignedToUserId == null && (!x.AssignedToUserId.IsDefined() || x.AssignedToUserId.IsNull()))) && (
                (x.ValidSince <= validUntil && x.ValidUntil >= validUntil) ||
                (x.ValidSince <= validSince && x.ValidUntil >= validSince) ||
                (x.ValidSince >= validSince && x.ValidUntil <= validUntil) ||
                (x.ValidSince <= validSince && x.ValidUntil >= validUntil))
            )
            .ToFeedIterator()
            .ExecuteQuery(logger, pageSizeOrMax, cancellationToken);
        return result;
    }
    public record LeaveLimitEntity(Guid Id, DateOnly ValidSince, DateOnly ValidUntil, string AssignedToUserId, Guid LeaveTypeId, [property: Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))] LeaveLimitState State);
}
