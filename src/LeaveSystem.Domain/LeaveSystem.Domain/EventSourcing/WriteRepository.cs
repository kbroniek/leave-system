namespace LeaveSystem.Domain.EventSourcing;
using System.Threading.Tasks;
using LeaveSystem.Shared;

public class WriteRepository(IAppendEventRepository appendEventRepository)
{
    internal async Task<Result<TEventSource, Error>> Write<TEventSource>(TEventSource eventSource, CancellationToken cancellationToken) where TEventSource : IEventSource, new()
    {
        foreach (var @event in eventSource.PendingEvents)
        {
            var result = await appendEventRepository.AppendToStreamAsync(@event, cancellationToken);
            if (!result.IsOk)
            {
                return result.Error;
            }
        }
        eventSource.PendingEvents.Clear();
        return eventSource;
    }
}
