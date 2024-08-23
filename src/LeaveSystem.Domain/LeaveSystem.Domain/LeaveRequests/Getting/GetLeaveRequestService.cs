namespace LeaveSystem.Domain.LeaveRequests.Getting;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;

public class GetLeaveRequestService(ReadRepository repository)
{
    public async Task<LeaveRequest> Get(Guid streamId, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync<LeaveRequest>(streamId, cancellationToken);
    }
}
