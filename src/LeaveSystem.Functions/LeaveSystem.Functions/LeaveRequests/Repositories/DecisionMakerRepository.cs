namespace LeaveSystem.Functions.LeaveRequests.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

internal class DecisionMakerRepository(CosmosClient cosmosClient, string databaseName, string containerId,
    ILogger<DecisionMakerRepository> logger) : IDecisionMakerRepository
{
    public async Task<Result<IReadOnlyCollection<string>, Error>> GetDecisionMakerUserIds(CancellationToken cancellationToken)
    {
        try
        {
            var container = cosmosClient.GetContainer(databaseName, containerId);
            var queryDefinition = new QueryDefinition($"SELECT c.id FROM c JOIN r IN c.roles WHERE r = \"{nameof(RoleType.DecisionMaker)}\"");
            var queryIterator = container.GetItemQueryIterator<DecisionMakerDto>(queryDefinition);

            var decisionMakerIds = new List<string>();
            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync(cancellationToken);
                decisionMakerIds.AddRange(response.Select(item => item.Id));
            }

            return Result.Ok<IReadOnlyCollection<string>, Error>(decisionMakerIds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting DecisionMaker user IDs");
            return new Error("Error occurred while getting DecisionMaker user IDs", System.Net.HttpStatusCode.InternalServerError, ErrorCodes.UNEXPECTED_GRAPH_ERROR);
        }
    }

    private sealed record DecisionMakerDto(string Id);
}
