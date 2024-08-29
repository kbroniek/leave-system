namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;

internal class LimitValidatorRepository : ILimitValidatorRepository
{
    public Task<(TimeSpan? limit, TimeSpan? overdueLimit)> GetLimit(DateOnly dateFrom, DateOnly dateTo, Guid leaveTypeId, string userId) => throw new NotImplementedException();
}
