namespace LeaveSystem.Domain.LeaveRequests.Getting;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Shared;

public class GetLeaveRequestService(ReadService readService)
{
    public async Task<Result<LeaveRequest, Error>> Get(Guid streamId, CancellationToken cancellationToken) =>
        await readService.FindByIdAsync<LeaveRequest>(streamId, cancellationToken);
}
