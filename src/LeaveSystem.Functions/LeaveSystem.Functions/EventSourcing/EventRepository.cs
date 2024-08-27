namespace LeaveSystem.Functions.EventSourcing;

using System.Net;
using System.Runtime.CompilerServices;
using LeaveSystem.Domain;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using static LeaveSystem.Functions.Config;

internal class EventRepository(CosmosClient cosmosClient, ILogger<EventRepository> logger, EventRepositorySettings settings)
    : IAppendEventRepository, IReadEventsRepository
{
    public async Task<Result<Error>> AppendToStreamAsync(IEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var container = cosmosClient.GetContainer(
                settings.DatabaseName ?? throw new InvalidOperationException("Event repository AppSettings DatabaseName configuration is missing. Check the appsettings.json."),
                settings.ContainerName ?? throw new InvalidOperationException("Event repository AppSettings ContainerName configuration is missing. Check the appsettings.json."));
            await container.CreateItemAsync(new EventModel<object>(
                Id: Guid.NewGuid(),
                StreamId: @event.StreamId,
                Body: @event,
                EventType: @event.GetType().AssemblyQualifiedName!
            ), new PartitionKey(@event.StreamId.ToString()), null, cancellationToken);
            return Result.Default;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            var errorMessage = "This event already exists";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage, HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            var errorMessage = "Unexpected error occurred while insert data to DB";
            logger.LogError(ex, "{Message}", errorMessage);
            return new Error(errorMessage, HttpStatusCode.InternalServerError);
        }
    }

    public async IAsyncEnumerable<IEvent> ReadStreamAsync(Guid streamId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var container = cosmosClient.GetContainer(settings.DatabaseName, settings.ContainerName);
        var sqlQuery = "SELECT * FROM c ORDER BY c._ts ASC";
        var iterator = container.GetItemQueryIterator<EventModel<JToken>>(sqlQuery, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(streamId.ToString()) });

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);

            foreach (var @event in response)
            {
                //TODO: Error handling
                yield return (IEvent)@event.Body.ToObject(Type.GetType(@event.EventType, true));
            }
        }
    }
}
