namespace LeaveSystem.Domain.EventSourcing;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;

public interface IAppendEventRepository
{
    Task<Result<Error>> AppendToStreamAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : notnull, IEvent;
}
