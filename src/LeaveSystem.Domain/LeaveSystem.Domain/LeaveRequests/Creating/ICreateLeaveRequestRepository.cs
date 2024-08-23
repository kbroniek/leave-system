namespace LeaveSystem.Domain.LeaveRequests.Creating;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;

public interface ICreateLeaveRequestRepository
{
    Task<Result<Error>> AppendToStreamAsync<TEvent>(TEvent @event) where TEvent : notnull, IEvent;

    public record Error(string? Message);
}
