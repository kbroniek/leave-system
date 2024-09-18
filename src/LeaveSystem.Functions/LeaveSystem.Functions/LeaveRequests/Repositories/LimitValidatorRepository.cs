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
    public async ValueTask<Result<(TimeSpan? limit, TimeSpan? overdueLimit, DateOnly? validSince, DateOnly? validUntil), Error>> GetLimit(
        DateOnly dateFrom, DateOnly dateTo, Guid leaveTypeId, string userId, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var limits = await container.GetItemLinqQueryable<Entity>()
            .Where(x =>
            (!x.AssignedToUserId.IsDefined() || x.AssignedToUserId.IsNull() || x.AssignedToUserId == userId) &&
            (!x.ValidSince.IsDefined() || x.ValidSince.IsNull() || x.ValidSince <= dateFrom) &&
            (!x.ValidUntil.IsDefined() || x.ValidUntil.IsNull() || x.ValidUntil >= dateTo) &&
            x.LeaveTypeId == leaveTypeId)
            .ToFeedIterator()
            .ExecuteQuery(cancellationToken);
        if (limits.Count == 0)
        {
            return new Error($"Cannot find limits for the leave type id: {leaveTypeId}. Add limits for the user {userId}.", System.Net.HttpStatusCode.UnprocessableEntity);
        }

        //Only assigned limits must be unique
        if (limits.Count(x => x.AssignedToUserId != null) > 1)
        {
            return new Error($"Two or more limits found which are the same for the leave type id: {leaveTypeId}. User {userId}.", System.Net.HttpStatusCode.UnprocessableEntity);
        }
        //Assigned limits has higher priority
        var limit = limits.OrderBy(x => x.AssignedToUserId).Last();
        return (limit.Limit, limit.OverdueLimit, limit.ValidSince, limit.ValidUntil);
    }
    private sealed record Entity(Guid LeaveTypeId, string? AssignedToUserId, DateOnly? ValidSince, DateOnly? ValidUntil, TimeSpan? Limit, TimeSpan? OverdueLimit);
}
