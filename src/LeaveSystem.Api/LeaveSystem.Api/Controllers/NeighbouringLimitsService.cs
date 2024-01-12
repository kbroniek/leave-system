using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Api.Controllers;

public class NeighbouringLimitsService
{
    private readonly LeaveSystemDbContext dbContext;

    public NeighbouringLimitsService(LeaveSystemDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public virtual async Task CloseNeighbourLimitsPeriodsAsync(
        UserLeaveLimit limit,
        CancellationToken cancellationToken = default)
    {
        var userLimitsForSameLeaveType = dbContext.UserLeaveLimits
            .Where(ull => ull.LeaveTypeId == limit.LeaveTypeId && ull.AssignedToUserId == limit.AssignedToUserId);
        if (limit.ValidSince.HasValue)
        {
            await ClosePreviousLimitPeriodAsync(userLimitsForSameLeaveType, limit.ValidSince.Value,
                cancellationToken);
        }

        if (limit.ValidUntil.HasValue)
        {
            await CloseNextLimitPeriodAsync(userLimitsForSameLeaveType, limit.ValidUntil.Value,
                cancellationToken);
        }
    }

    private async Task ClosePreviousLimitPeriodAsync(
        IQueryable<UserLeaveLimit> userLeaveLimits,
        DateTimeOffset validSince,
        CancellationToken cancellationToken)
    {
        var limitWithPeriodToClose = await userLeaveLimits
            .OrderBy(x => x.ValidSince)
            .LastOrDefaultAsync(
                x => !x.ValidUntil.HasValue && x.ValidSince < validSince
                , cancellationToken);
        if (limitWithPeriodToClose is null)
        {
            return;
        }

        limitWithPeriodToClose.ValidUntil = validSince.AddDays(-1);
        dbContext.Update(limitWithPeriodToClose);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CloseNextLimitPeriodAsync(
        IQueryable<UserLeaveLimit> userLeaveLimits,
        DateTimeOffset validUntil,
        CancellationToken cancellationToken)
    {
        var limitWithPeriodToClose = await userLeaveLimits
            .OrderBy(x => x.ValidSince)
            .FirstOrDefaultAsync(
                x => !x.ValidSince.HasValue && x.ValidUntil > validUntil
                , cancellationToken);
        if (limitWithPeriodToClose is null)
        {
            return;
        }

        limitWithPeriodToClose.ValidSince = validUntil.AddDays(1);
        dbContext.Update(limitWithPeriodToClose);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}