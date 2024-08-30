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
        var connectedLeaveTypeIdsResult = await connectedLeaveTypesRepository.GetConnectedLeaveTypeIds(leaveTypeId, cancellationToken);
        if (connectedLeaveTypeIdsResult.IsFailure)
        {
            return connectedLeaveTypeIdsResult.Error;
        }
        (var nestedLeaveTypeIds, var baseLeaveTypeId) = connectedLeaveTypeIdsResult.Value;
        var checkLimitResult = await CheckLimitForBaseLeave(dateFrom,
            dateTo,
            userId,
            leaveTypeId,
            duration,
            nestedLeaveTypeIds,
            cancellationToken);
        if (checkLimitResult.IsFailure)
        {
            return checkLimitResult;
        }

        if (baseLeaveTypeId != null)
        {
            var baseCheckLimitResult = await CheckLimitForBaseLeave(dateFrom,
                dateTo,
                userId,
                baseLeaveTypeId.Value,
                duration,
                [],
                cancellationToken);
            if (baseCheckLimitResult.IsFailure)
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
        IEnumerable<Guid> nestedLeaveTypeIds,
        CancellationToken cancellationToken)
    {
        var result = await leaveLimitsRepository.GetLimit(
            dateFrom, dateTo,
            leaveTypeId, userId,
            cancellationToken);
        if (result.IsFailure)
        {
            return result.Error;
        }
        var (limit, overdueLimit) = result.Value;
        if (limit is not null)
        {
            var totalUsed = await usedLeavesRepository.GetUsedLeavesDuration(
                dateFrom, dateTo,
                userId, leaveTypeId,
                nestedLeaveTypeIds,
                cancellationToken);
            if (CalculateRemaningLimit(limit.Value, overdueLimit, totalUsed + duration) < TimeSpan.Zero)
            {
                return new Error($"You don't have enough free days for this type of leave. LeaveTypeId={leaveTypeId}", System.Net.HttpStatusCode.UnprocessableEntity);
            }
        }
        return Result.Default;
    }

    private static TimeSpan CalculateRemaningLimit(TimeSpan limit, TimeSpan? overdueLimit, TimeSpan usedLimits) =>
        limit + (overdueLimit ?? TimeSpan.Zero) - usedLimits;
}

public interface IConnectedLeaveTypesRepository
{
    ValueTask<Result<(IEnumerable<Guid> nestedLeaveTypeIds, Guid? baseLeaveTypeId), Error>> GetConnectedLeaveTypeIds(Guid leaveTypeId, CancellationToken cancellationToken);
}

public interface IUsedLeavesRepository
{
    ValueTask<TimeSpan> GetUsedLeavesDuration(DateOnly dateFrom, DateOnly dateTo, string userId, Guid leaveTypeId, IEnumerable<Guid> nestedLeaveTypeIds, CancellationToken cancellationToken);
}

public interface ILimitValidatorRepository
{
    ValueTask<Result<(TimeSpan? limit, TimeSpan? overdueLimit), Error>> GetLimit(DateOnly dateFrom, DateOnly dateTo, Guid leaveTypeId, string userId, CancellationToken cancellationToken);
}
