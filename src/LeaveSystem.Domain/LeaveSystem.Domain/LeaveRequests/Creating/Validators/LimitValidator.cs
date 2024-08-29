namespace LeaveSystem.Domain.LeaveRequests.Creating.Validators;

using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared;

public class LimitValidator(ILimitValidatorRepository leaveLimitsRepository, IUsedLeavesRepository usedLeavesRepository, IConnectedLeaveTypesRepository connectedLeaveTypesRepository)
{
    public virtual async Task<Result<Error>> Validate(LeaveRequestCreated creatingLeaveRequest)
    {
        var connectedLeaveTypeIds = await connectedLeaveTypesRepository.GetConnectedLeaveTypeIds(creatingLeaveRequest.LeaveTypeId);
        var checkLimitResult = await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
            creatingLeaveRequest.DateTo,
            creatingLeaveRequest.CreatedBy.Id,
            creatingLeaveRequest.LeaveTypeId,
            creatingLeaveRequest.Duration,
            connectedLeaveTypeIds.nestedLeaveTypeIds);
        if (!checkLimitResult.IsSuccess)
        {
            return checkLimitResult;
        }

        if (connectedLeaveTypeIds.baseLeaveTypeId != null)
        {
            var baseLeaveTypeId = connectedLeaveTypeIds.baseLeaveTypeId.Value;
            var baseCheckLimitResult = await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
                creatingLeaveRequest.DateTo,
                creatingLeaveRequest.CreatedBy.Id,
                baseLeaveTypeId,
                creatingLeaveRequest.Duration,
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
