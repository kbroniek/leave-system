namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;

internal class RolesRepository(CosmosClient cosmosClient, string databaseName, string containerId) : IRolesRepository
{
    public async Task<Result<IRolesRepository.UserRoles, Error>> GetUserRoles(string id, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var results = await container.GetItemQueryIterator<RolesEntity>(requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(id) })
            .ExecuteQuery(cancellationToken);
        if (results.Count == 0)
        {
            return new Error($"Cannot find roles. UserId={id}", System.Net.HttpStatusCode.NotFound);
        }
        if (results.Count > 1)
        {
            return new Error($"More than one role for the user. UserId={id}", System.Net.HttpStatusCode.UnprocessableEntity);
        }
        return results.Select(x => new IRolesRepository.UserRoles(x.Roles)).First();
    }
    private sealed record RolesEntity(string[] Roles);
}
