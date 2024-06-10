namespace LeaveSystem.Functions.EventSourcing.LeaveRequests;

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

internal class EventRepository(CosmosClient cosmosClient, ILogger<EventRepository> logger) : IDisposable
{
    private readonly CosmosClient cosmosClient = cosmosClient;
    public void Dispose() => this.cosmosClient.Dispose();

    public async Task AppendAsync(Event @event)
    {
        try
        {
            var container = this.cosmosClient.GetContainer("LeaveSystem", "Events");
            var response = await container.CreateItemAsync(@event);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // Poprawić resulta na feature-azure-functions
            logger.LogError(ex, "");
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {

        }
        catch (Exception ex)
        {

        }
    }
}
