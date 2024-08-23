namespace LeaveSystem.Functions.EventSourcing;

using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static LeaveSystem.Domain.LeaveRequests.IAppendEventRepository;
using static LeaveSystem.Functions.Config;

internal class EventRepository(CosmosClient cosmosClient, ILogger<EventRepository> logger, EventRepositorySettings settings)
    : IAppendEventRepository, IReadEventsRepository
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
    public async IAsyncEnumerable<IEvent> ReadStreamAsync(Guid streamId)
    {
        var container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);
        var sqlQuery = "SELECT * FROM c ORDER BY c._ts ASC";
        var iterator = container.GetItemQueryIterator<EventModel<JToken>>(sqlQuery, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(streamId.ToString()) });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();

            foreach (var @event in response)
            {
                //TODO: Error handling
                yield return (IEvent)@event.Body.ToObject(Type.GetType(@event.EventType, true));
            }
        }
    }
}
