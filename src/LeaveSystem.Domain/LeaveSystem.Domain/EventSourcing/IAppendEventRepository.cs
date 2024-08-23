namespace LeaveSystem.Domain.EventSourcing;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Shared;

public interface IAppendEventRepository
{
    Task<Result<Error>> AppendToStreamAsync(IEvent @event, CancellationToken cancellationToken);
}
