namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;

using LeaveSystem.Domain;
using LeaveSystem.Shared;

public class LimitValidator(ILimitValidatorRepository leaveLimitsRepository, IUsedLeavesRepository usedLeavesRepository, IConnectedLeaveTypesRepository connectedLeaveTypesRepository)
{
    public virtual async Task<Result<Error>> Validate(
        DateOnly dateFrom,
        DateOnly dateTo,
        TimeSpan duration,
        Guid leaveTypeId,
        TimeSpan workingHours,
        string userId,
        CancellationToken cancellationToken)
    {
        var connectedLeaveTypeIds = await connectedLeaveTypesRepository.GetConnectedLeaveTypeIds(leaveTypeId);
        var checkLimitResult = await CheckLimitForBaseLeave(dateFrom,
            dateTo,
            userId,
            leaveTypeId,
            duration,
            connectedLeaveTypeIds.nestedLeaveTypeIds);
        if (!checkLimitResult.IsSuccess)
        {
            return checkLimitResult;
        }

        if (connectedLeaveTypeIds.baseLeaveTypeId != null)
        {
            var baseLeaveTypeId = connectedLeaveTypeIds.baseLeaveTypeId.Value;
            var baseCheckLimitResult = await CheckLimitForBaseLeave(dateFrom,
                dateTo,
                userId,
                baseLeaveTypeId,
                duration,
                []);
            if (!baseCheckLimitResult.IsSuccess)
            {
                return baseCheckLimitResult;
            }
        }
        return Result.Default;
    }

    private async Task<Result<Error>> CheckLimitForBaseLeave(
        DateOnly dateFrom,
        DateOnly dateTo,
        string userId,
        Guid leaveTypeId,
        TimeSpan duration,
        IEnumerable<Guid> nestedLeaveTypeIds)
    {
        var (limit, overdueLimit) = await leaveLimitsRepository.GetLimit(dateFrom,
            dateTo,
            leaveTypeId,
            userId);
        var totalUsed = await usedLeavesRepository.GetUsedLeavesDuration(
            dateFrom,
            dateFrom,
            userId,
            leaveTypeId,
            nestedLeaveTypeIds);
        if (limit != null &&
            CalculateRemaningLimit(limit.Value, overdueLimit, totalUsed + duration) < TimeSpan.Zero)
        {
            return new Error("You don't have enough free days for this type of leave", System.Net.HttpStatusCode.BadRequest);
        }
        return Result.Default;
    }

    private static TimeSpan CalculateRemaningLimit(TimeSpan limit, TimeSpan? overdueLimit, TimeSpan usedLimits) =>
        limit + (overdueLimit ?? TimeSpan.Zero) - usedLimits;
}

public interface IConnectedLeaveTypesRepository
{
    ValueTask<(IEnumerable<Guid> nestedLeaveTypeIds, Guid? baseLeaveTypeId)> GetConnectedLeaveTypeIds(Guid leaveTypeId);
}

public interface IUsedLeavesRepository
{
    ValueTask<TimeSpan> GetUsedLeavesDuration(DateOnly dateFrom1, DateOnly dateFrom2, string userId, Guid leaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds);
}

public interface ILimitValidatorRepository
{
    Task<(TimeSpan? limit, TimeSpan? overdueLimit)> GetLimit(DateOnly dateFrom, DateOnly dateTo, Guid leaveTypeId, string userId);
}
