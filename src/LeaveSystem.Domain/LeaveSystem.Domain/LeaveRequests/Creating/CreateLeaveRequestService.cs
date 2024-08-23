namespace LeaveSystem.Domain.LeaveRequests.Creating;
using System.Threading.Tasks;
using LeaveSystem.Shared;
using static LeaveSystem.Domain.LeaveRequests.Creating.ICreateLeaveRequestRepository;

public class CreateLeaveRequestService(ICreateLeaveRequestRepository createLeaveRequestRepository)
{
    public async Task<Result<Error>> Create(LeaveRequestCreated leaveRequestCreated)
    {
        return await createLeaveRequestRepository.AppendToStreamAsync(leaveRequestCreated);
    }
}
