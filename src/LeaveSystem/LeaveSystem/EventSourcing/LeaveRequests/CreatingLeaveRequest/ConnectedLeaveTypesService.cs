using LeaveSystem.Db;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class ConnectedLeaveTypesService
{
    private readonly LeaveSystemDbContext dbContext;

    public ConnectedLeaveTypesService(LeaveSystemDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public virtual async Task<(Guid? baseLeaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds)> GetConnectedLeaveTypeIds(Guid leaveTypeId)
    {
        var leaveTypes = await dbContext.LeaveTypes.Where(l =>
            l.BaseLeaveTypeId == leaveTypeId || l.Id == leaveTypeId).ToListAsync();
        if (leaveTypes.Count == 0)
        {
            return (null, null);
        }

        var nestedLeaveTypes = leaveTypes.Where(l => l.BaseLeaveTypeId == leaveTypeId);
        var currentLeaveType = leaveTypes.FirstOrDefault(l => l.Id == leaveTypeId);
        return (currentLeaveType?.BaseLeaveTypeId, nestedLeaveTypes.Select(x => x.Id));
    }
}
