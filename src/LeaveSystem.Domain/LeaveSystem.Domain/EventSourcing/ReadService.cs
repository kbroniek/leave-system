namespace LeaveSystem.Domain.EventSourcing;

using System.Net;
using LeaveSystem.Shared;
using Microsoft.Extensions.Logging;

public class ReadService(IReadEventsRepository readEventsRepository, ILogger<ReadService> logger)
{
    internal virtual async Task<Result<TEventSource, Error>> FindById<TEventSource>(Guid id, CancellationToken cancellationToken) where TEventSource : IEventSource, new()
    {
        var leaveRequest = new TEventSource();
        var i = 0;
        await foreach (var item in readEventsRepository
            .ReadStreamAsync(id, cancellationToken)
            .WithCancellation(cancellationToken))
        {
            leaveRequest.Evolve(item);
            ++i;
        }
        if (i == 0)
        {
            var errorMessage = $"Cannot find the resource id {id}";
            logger.LogError("{ErrorMessage}", errorMessage);
            return new Error(errorMessage, HttpStatusCode.NotFound, ErrorCodes.RESOURCE_NOT_FOUND);
        }
        return leaveRequest;
    }
    internal virtual async Task<IEnumerable<TEventSource>> FindByIds<TEventSource>(Guid[] ids, CancellationToken cancellationToken) where TEventSource : IEventSource, new()
    {
        List<IEvent> events = [];
        await foreach (var item in readEventsRepository
            .ReadStreamAsync(ids, cancellationToken)
            .WithCancellation(cancellationToken))
        {
            events.Add(item);
        }

        return CreateEventSource<TEventSource>(events);
    }

    private static IEnumerable<TEventSource> CreateEventSource<TEventSource>(List<IEvent> events) where TEventSource : IEventSource, new()
    {
        var groupByStreamId = events.GroupBy(x => x.StreamId);
        foreach (var eventsByStream in groupByStreamId)
        {
            var leaveRequest = new TEventSource();
            foreach (var @event in eventsByStream)
            {
                leaveRequest.Evolve(@event);
            }
            yield return leaveRequest;
        }
    }
}
