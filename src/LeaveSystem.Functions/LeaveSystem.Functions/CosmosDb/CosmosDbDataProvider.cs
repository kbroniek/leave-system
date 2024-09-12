namespace LeaveSystem.Functions.CosmosDb;

using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

internal class CosmosDbDataProvider(CosmosClient cosmosClient,
    ILogger<CosmosDbDataProvider> logger,
    string databaseName,
    string eventsContainerId)
{
    public async IAsyncEnumerable<T> Get<T>(Guid streamId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(databaseName, eventsContainerId);
        var sqlQuery = "SELECT * FROM c ORDER BY c._ts ASC";
        var iterator = container.GetItemQueryIterator<T>(sqlQuery, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(streamId.ToString()) });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);

            foreach (var @event in response)
            {
                yield return @event;
            }
        }
    }
}
