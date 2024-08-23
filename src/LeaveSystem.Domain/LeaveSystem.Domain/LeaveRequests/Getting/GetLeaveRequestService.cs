namespace LeaveSystem.Domain.LeaveRequests.Getting;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests;

public class GetLeaveRequestService(IReadEventsRepository repository)
{
    public async Task<LeaveRequest> Get(Guid streamId)
    {
        var leaveRequest = LeaveRequest.Default();
        await foreach (var item in repository.ReadStreamAsync(streamId))
        {
            LeaveRequest.Evolve(leaveRequest, item);
        }
        return leaveRequest;
    }
}
