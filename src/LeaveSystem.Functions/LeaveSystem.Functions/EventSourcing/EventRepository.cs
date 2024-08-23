namespace LeaveSystem.Functions.EventSourcing;

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Getting;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static LeaveSystem.Domain.LeaveRequests.Creating.ICreateLeaveRequestRepository;
using static LeaveSystem.Functions.Config;

internal class EventRepository(CosmosClient cosmosClient, ILogger<EventRepository> logger, EventRepositorySettings settings)
    : ICreateLeaveRequestRepository, IGetLeaveRequestRepository
{
    public async Task<Result<Error>> AppendToStreamAsync<TEvent>(TEvent @event) where TEvent : notnull, IEvent
    {
        try
        {
            var container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);
            await container.CreateItemAsync(new EventModel<TEvent>(
                Id: Guid.NewGuid(),
                StreamId: @event.StreamId,
                Body: @event,
                EventType: @event.GetType().AssemblyQualifiedName!
            ));
            return Result.Default;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            var errorMessage = "This event already exists";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while insert data to DB";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage);
        }
    }
    //public TEvent[] ReadStream<TEvent>(Guid streamId) where TEvent : notnull =>
    //    events.TryGetValue(streamId, out var stream)
    //        ? stream.Select(@event =>
    //                JsonSerializer.Deserialize(@event.Json, Type.GetType(@event.EventType, true)!)
    //            )
    //            .Where(e => e != null).Cast<TEvent>().ToArray()
    //        : [];
    public async IAsyncEnumerable<object> ReadStreamAsync(Guid streamId)
    {
        var container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);
        //using var iterator = container.GetItemQueryIterator<JsonDocument>(
        //    requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(streamId.ToString()) });

        var queryable = container.GetItemLinqQueryable<EventModel<JToken>>();

        // Construct LINQ query
        var matches = queryable
            .Where(p => p.StreamId == streamId);

        // Convert to feed iterator
        using var linqFeed = matches.ToFeedIterator();

        // Iterate query result pages
        while (linqFeed.HasMoreResults)
        {
            var response = await linqFeed.ReadNextAsync();

            // Iterate query results
            foreach (var @event in response)
            {
                yield return @event.Body.ToObject(Type.GetType(@event.EventType, true));
            }
        }
    }
}
