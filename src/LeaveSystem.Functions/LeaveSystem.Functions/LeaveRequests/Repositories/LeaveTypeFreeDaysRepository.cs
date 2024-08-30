namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

public class LeaveTypeFreeDaysRepository(CosmosClient cosmosClient, string databaseName, string containerId) : ILeaveTypeFreeDaysRepository
{
    public async Task<Result<bool?, Error>> IsIncludeFreeDays(Guid leaveTypeId, CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, containerId);
        var iterator = container.GetItemLinqQueryable<Entity>()
            .Where(x => x.Id == leaveTypeId)
            .ToFeedIterator();

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            return response.First().Properties.IncludeFreeDays;
        }
        return new Error($"Cannot find the leave type. Id={leaveTypeId}", System.Net.HttpStatusCode.NotFound);
    }

    private record Entity(Guid Id, EntityProperties Properties);

    private record EntityProperties(bool? IncludeFreeDays);
}
