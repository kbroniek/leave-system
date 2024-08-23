namespace LeaveSystem.Domain.LeaveRequests.Accepting;
using System.Threading.Tasks;
using LeaveSystem.Shared;
using static LeaveSystem.Domain.LeaveRequests.IAppendEventRepository;

public class AcceptLeaveRequestService(IAppendEventRepository repository)
{
    public async Task<Result<Error>> Create(LeaveRequestAccepted leaveRequestAccepted)
    {
        return await repository.AppendToStreamAsync(leaveRequestAccepted);
    }
}
