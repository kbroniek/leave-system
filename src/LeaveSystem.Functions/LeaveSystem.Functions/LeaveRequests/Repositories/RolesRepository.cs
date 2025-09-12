namespace LeaveSystem.Functions.LeaveRequests.Repositories;

using System.Net;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

internal class RolesRepository(CosmosClient cosmosClient, string databaseName, string containerId,
    ILogger<RolesRepository> logger) : IRolesRepository
{
    public async Task<Result<IRolesRepository.UserRoles, Error>> GetUserRoles(string id, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var results = await container.GetItemQueryIterator<RolesEntity>(requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(id) })
            .ExecuteQuery(logger, cancellationToken);
        if (results.IsFailure)
        {
            logger.LogWarning("{Message}", $"Error occurred while getting user userId {id}");
            return results.Error;
        }
        var roles = results.Value;
        if (roles.Count == 0)
        {
            return new Error($"Cannot find roles. UserId={id}", HttpStatusCode.NotFound, ErrorCodes.ROLES_NOT_FOUND);
        }
        if (roles.Count > 1)
        {
            return new Error($"More than one role for the user. UserId={id}", HttpStatusCode.UnprocessableEntity, ErrorCodes.DUPLICATE_ROLES);
        }
        return roles.Select(x => new IRolesRepository.UserRoles(x.Roles)).First();
    }
    private sealed record RolesEntity(string[] Roles);
}
