namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;

public class LeaveTypeFreeDaysRepository : ILeaveTypeFreeDaysRepository
{
    public Task<Result<bool?, Error>> IsIncludeFreeDays(Guid leaveTypeId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
