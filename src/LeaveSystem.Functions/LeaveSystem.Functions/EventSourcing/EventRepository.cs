namespace LeaveSystem.Functions.EventSourcing;

using System.Net;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using static LeaveSystem.Functions.Config;

internal class EventRepository(CosmosClient cosmosClient, ILogger<EventRepository> logger, EventRepositorySettings settings)
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
                EventType: @event.GetType().FullName!
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
    public async IAsyncEnumerable<TEvent> ReadStreamAsync<TEvent>(Guid streamId) where TEvent : notnull, IEvent
    {
        var container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);
        var queryable = container.GetItemLinqQueryable<EventModel<TEvent>>();

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
            foreach (var item in response)
            {
                yield return item.Body;
            }
        }
    }

    internal record Error(string? Message)
    {
    }
}
