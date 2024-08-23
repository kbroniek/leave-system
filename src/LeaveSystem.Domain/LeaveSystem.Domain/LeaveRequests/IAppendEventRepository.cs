namespace LeaveSystem.Domain.LeaveRequests;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;

public interface IAppendEventRepository
{
    Task<Result<Error>> AppendToStreamAsync<TEvent>(TEvent @event) where TEvent : notnull, IEvent;

    public record Error(string? Message);
}
