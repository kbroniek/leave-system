namespace LeaveSystem.Functions.EventSourcing.LeaveRequests;

using System.Net;
using Microsoft.Azure.Cosmos;

internal class EventRepository : IDisposable
{
    private readonly CosmosClient cosmosClient;

    public EventRepository(CosmosClient cosmosClient) => this.cosmosClient = cosmosClient;

    public void Dispose() => this.cosmosClient.Dispose();

    public async Task AppendAsync(Event @event)
    {
        var container = this.cosmosClient.GetContainer("", "");
        var response = await container.CreateItemAsync(@event);
        response.StatusCode == HttpStatusCode.OK
    }

}
