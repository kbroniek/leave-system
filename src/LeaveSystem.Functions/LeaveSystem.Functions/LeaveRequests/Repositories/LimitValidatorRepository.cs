namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

internal class LimitValidatorRepository(CosmosClient cosmosClient, string databaseName, string containerId) : ILimitValidatorRepository
{
    public async ValueTask<Result<(TimeSpan? limit, TimeSpan? overdueLimit), Error>> GetLimit(
        DateOnly dateFrom, DateOnly dateTo, Guid leaveTypeId, string userId, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var iterator = container.GetItemLinqQueryable<Entity>()
            .Where(x =>
            (x.AssignedToUserId == null || x.AssignedToUserId == userId) &&
            (x.ValidSince == null || x.ValidSince <= dateFrom) &&
            (x.ValidUntil == null || x.ValidUntil >= dateTo) &&
            x.LeaveTypeId == leaveTypeId)
            .ToFeedIterator();
        var limits = await iterator.ExecuteQuery(cancellationToken);
        if (limits == null || limits.Count == 0)
        {
            return new Error($"Cannot find limits for the leave type id: {leaveTypeId}. Add limits for the user {userId}.", System.Net.HttpStatusCode.UnprocessableEntity);
        }

        if (limits.Count > 1)
        {
            return new Error($"Two or more limits found which are the same for the leave type id: {leaveTypeId}. User {userId}.", System.Net.HttpStatusCode.UnprocessableEntity);
        }
        var limit = limits[0];
        return (limit.Limit, limit.OverdueLimit);
    }
    private sealed record Entity(Guid LeaveTypeId, string? AssignedToUserId, DateOnly? ValidSince, DateOnly? ValidUntil, TimeSpan Limit, TimeSpan OverdueLimit);
}
