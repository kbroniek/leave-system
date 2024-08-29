using System.ComponentModel.DataAnnotations;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

internal class LimitValidator(ILimitValidatorRepository leaveLimitsService, UsedLeavesService usedLeavesService, ConnectedLeaveTypesService connectedLeaveTypesService)
{
    public virtual async Task Validate(LeaveRequestCreated creatingLeaveRequest)
    {
        var connectedLeaveTypeIds = await connectedLeaveTypesService.GetConnectedLeaveTypeIds(creatingLeaveRequest.LeaveTypeId);
        await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
            creatingLeaveRequest.DateTo,
            creatingLeaveRequest.CreatedBy.Id,
            creatingLeaveRequest.LeaveTypeId,
            creatingLeaveRequest.Duration,
            connectedLeaveTypeIds.nestedLeaveTypeIds);

        if (connectedLeaveTypeIds.baseLeaveTypeId != null)
        {
            var baseLeaveTypeId = connectedLeaveTypeIds.baseLeaveTypeId.Value;
            await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
                creatingLeaveRequest.DateTo,
                creatingLeaveRequest.CreatedBy.Id,
                baseLeaveTypeId,
                creatingLeaveRequest.Duration,
                Enumerable.Empty<Guid>());
        }
    }

    private async Task CheckLimitForBaseLeave(
        DateOnly dateFrom,
        DateOnly dateTo,
        string userId,
        Guid leaveTypeId,
        TimeSpan duration,
        IEnumerable<Guid> nestedLeaveTypeIds)
    {
        var leaveLimit = await leaveLimitsService.GetLimit(dateFrom,
            dateTo,
            leaveTypeId,
            userId);
        var totalUsed = await usedLeavesService.GetUsedLeavesDuration(
            dateFrom,
            dateFrom,
            userId,
            leaveTypeId,
            nestedLeaveTypeIds);
        if (leaveLimit.Limit != null &&
            CalculateRemaningLimit(leaveLimit.Limit.Value, leaveLimit.OverdueLimit, totalUsed + duration) < TimeSpan.Zero)
        {
            throw new ValidationException("You don't have enough free days for this type of leave");
        }
    }

    private static TimeSpan CalculateRemaningLimit(TimeSpan limit, TimeSpan? overdueLimit, TimeSpan usedLimits) =>
        limit + (overdueLimit ?? TimeSpan.Zero) - usedLimits;
}

internal class ConnectedLeaveTypesService
{
    internal async Task<IEnumerable<Guid>> GetConnectedLeaveTypeIds(Guid leaveTypeId) => throw new NotImplementedException();
}

internal class UsedLeavesService
{
}
