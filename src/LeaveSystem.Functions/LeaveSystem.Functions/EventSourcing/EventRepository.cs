namespace LeaveSystem.Functions.EventSourcing;

using System.Net;
using LeaveSystem.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using static LeaveSystem.Functions.Config;

internal class EventRepository
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<EventRepository> _logger;
    private readonly EventRepositorySettings _settings;
    private readonly TimeProvider _timeProvider;

    internal EventRepository(CosmosClient cosmosClient, ILogger<EventRepository> logger, EventRepositorySettings settings)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
        _settings = settings;
    }

    public async Task<Result<EventRepositoryException>> AppendAsync(Event @event)
    {
        if (@event.CreatedAt > _timeProvider.GetUtcNow())
        {
            _logger.LogError("Can't create event {EventId} in past time {Date}", @event.StreamId, @event.CreatedAt);
            return new EventRepositoryException("You can't create events in the future");
        }
        try
        {
            var container = _cosmosClient.GetContainer(_settings.DatabaseName, _settings.ContainerName);
            var response = await container.CreateItemAsync(@event);
            return new Result<EventRepositoryException>();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            var errorMessage = "This event already exists";
            _logger.LogError(ex, "{Message}", errorMessage);
            return new EventRepositoryException(errorMessage);
        }
        catch (CosmosException ex)
        {
            var errorMessage = "Unexpected error occured";
            _logger.LogError(ex, "{Message}", errorMessage);
            return new EventRepositoryException(errorMessage);
        }
    }
}

internal class EventRepositoryException(string? message) : Exception(message)
{
}
