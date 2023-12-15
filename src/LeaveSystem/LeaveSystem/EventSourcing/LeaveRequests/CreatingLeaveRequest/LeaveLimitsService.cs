using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using System.ComponentModel.DataAnnotations;
using EFExtensions = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LeaveLimitsService
{
    private readonly LeaveSystemDbContext dbContext;

    public LeaveLimitsService(LeaveSystemDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    public virtual async Task<UserLeaveLimit> GetLimits(
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        Guid leaveTypeId,
        string userId)
    {
        var limits = await EFExtensions.ToListAsync(dbContext.UserLeaveLimits.Where(l =>
            (l.AssignedToUserId == null || l.AssignedToUserId == userId) &&
            (l.ValidSince == null || l.ValidSince <= dateFrom) &&
            (l.ValidUntil == null || l.ValidUntil >= dateTo) &&
            l.LeaveTypeId == leaveTypeId));
        if (limits == null || limits.Count == 0)
        {
            throw new ValidationException(
                $"Cannot find limits for the leave type id: {leaveTypeId}. Add limits for the user {userId}.");
        }

        if (limits.Count > 1)
        {
            throw new ValidationException(
                $"Two or more limits found which are the same for the leave type id: {leaveTypeId}. User {userId}.");
        }

        return limits.First();
    }
}
