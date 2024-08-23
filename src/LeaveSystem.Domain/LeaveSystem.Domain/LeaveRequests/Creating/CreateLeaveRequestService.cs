namespace LeaveSystem.Domain.LeaveRequests.Creating;
using System.Threading.Tasks;
using LeaveSystem.Shared;
using static LeaveSystem.Domain.LeaveRequests.IAppendEventRepository;

public class CreateLeaveRequestService(IAppendEventRepository repository)
{
    public async Task<Result<Error>> Create(LeaveRequestCreated leaveRequestCreated)
    {
        return await repository.AppendToStreamAsync(leaveRequestCreated);
    }
}
