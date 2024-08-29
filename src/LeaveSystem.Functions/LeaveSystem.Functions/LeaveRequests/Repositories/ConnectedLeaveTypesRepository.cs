namespace LeaveSystem.Functions.LeaveRequests.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;

internal class ConnectedLeaveTypesRepository : IConnectedLeaveTypesRepository
{
    public ValueTask<(IEnumerable<Guid> nestedLeaveTypeIds, Guid? baseLeaveTypeId)> GetConnectedLeaveTypeIds(Guid leaveTypeId) => throw new NotImplementedException();
}
