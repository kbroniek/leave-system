namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;

internal class UsedLeavesRepository : IUsedLeavesRepository
{
    public ValueTask<TimeSpan> GetUsedLeavesDuration(DateOnly dateFrom1, DateOnly dateFrom2, string userId, Guid leaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds) => throw new NotImplementedException();
}
