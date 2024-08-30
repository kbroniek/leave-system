namespace LeaveSystem.Domain.EventSourcing;

using System.Net;
using LeaveSystem.Shared;
using Microsoft.Extensions.Logging;

public class ReadService(IReadEventsRepository readEventsRepository, ILogger<ReadService> logger)
{
    internal virtual async Task<Result<TEventSource, Error>> FindByIdAsync<TEventSource>(Guid id, CancellationToken cancellationToken) where TEventSource : IEventSource, new()
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
            return new Error(errorMessage, HttpStatusCode.NotFound);
        }
        return leaveRequest;
    }
}
