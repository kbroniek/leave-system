namespace LeaveSystem.Domain.LeaveRequests.Getting;
using System;
using System.Threading.Tasks;

public class GetLeaveRequestService(IGetLeaveRequestRepository getLeaveRequestRepository)
{
    public async Task Get(Guid streamId)
    {
        await foreach (var item in getLeaveRequestRepository.ReadStreamAsync(streamId))
        {

        }
    }
}
