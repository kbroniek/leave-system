namespace LeaveSystem.Domain.EventSourcing;
public class ReadRepository(IReadEventsRepository readEventsRepository)
{
    internal async Task<TEventSource> FindByIdAsync<TEventSource>(Guid id, CancellationToken cancellationToken) where TEventSource : IEventSource, new()
    {
        var leaveRequest = new TEventSource();
        await foreach (var item in readEventsRepository.ReadStreamAsync(id, cancellationToken).WithCancellation(cancellationToken))
        {
            leaveRequest.Evolve(item);
        }
        return leaveRequest;
    }
}
