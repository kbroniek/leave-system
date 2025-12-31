namespace LeaveSystem.Domain.EventSourcing;
using System.Threading.Tasks;
using LeaveSystem.Shared;

public class WriteService(IAppendEventRepository appendEventRepository)
{
    internal virtual async Task<Result<TEventSource, Error>> Write<TEventSource>(TEventSource eventSource, CancellationToken cancellationToken) where TEventSource : IEventSource, new()
    {
        while (eventSource.PendingEvents.Count > 0)
        {
            var @event = eventSource.PendingEvents.Dequeue();
            var result = await appendEventRepository.AppendToStreamAsync(@event, cancellationToken);
            if (!result.IsSuccess)
            {
                eventSource.PendingEvents.Enqueue(@event);
                return result.Error;
            }
        }
        return eventSource;
    }
}
