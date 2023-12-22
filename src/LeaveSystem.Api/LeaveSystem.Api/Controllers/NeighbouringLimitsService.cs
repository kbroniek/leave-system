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
        CancellationToken cancellationToken)
    {
        var userLimitsForSameLeaveType = dbContext.UserLeaveLimits
            .Where(ull => ull.LeaveTypeId == limit.LeaveTypeId && ull.AssignedToUserId == limit.AssignedToUserId);
        ;
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
        var limitWithoutEndDateBeforeThisLimit = await userLeaveLimits
            .OrderBy(x => x.ValidSince)
            .LastOrDefaultAsync(
                x => !x.ValidUntil.HasValue && x.ValidSince < validSince
                , cancellationToken);
        if (limitWithoutEndDateBeforeThisLimit is null)
        {
            return;
        }

        limitWithoutEndDateBeforeThisLimit.ValidUntil = validSince;
        dbContext.Update(limitWithoutEndDateBeforeThisLimit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CloseNextLimitPeriodAsync(
        IQueryable<UserLeaveLimit> userLeaveLimits,
        DateTimeOffset validUntil,
        CancellationToken cancellationToken)
    {
        var limitWithoutStartDateAfterThisLimit = await userLeaveLimits
            .OrderBy(x => x.ValidSince)
            .FirstOrDefaultAsync(
                x => !x.ValidSince.HasValue && x.ValidUntil > validUntil
                , cancellationToken);
        if (limitWithoutStartDateAfterThisLimit is null)
        {
            return;
        }

        limitWithoutStartDateAfterThisLimit.ValidSince = validUntil;
        dbContext.Update(limitWithoutStartDateAfterThisLimit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}