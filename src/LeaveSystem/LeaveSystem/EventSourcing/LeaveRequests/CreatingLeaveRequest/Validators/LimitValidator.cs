using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using System.ComponentModel.DataAnnotations;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class LimitValidator
{
    private readonly LeaveLimitsService leaveLimitsService;
    private readonly UsedLeavesService usedLeavesService;
    private readonly ConnectedLeaveTypesService connectedLeaveTypesService;

    public LimitValidator(LeaveLimitsService leaveLimitsService, UsedLeavesService usedLeavesService, ConnectedLeaveTypesService connectedLeaveTypesService)
    {
        this.leaveLimitsService = leaveLimitsService;
        this.usedLeavesService = usedLeavesService;
        this.connectedLeaveTypesService = connectedLeaveTypesService;
    }
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
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
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
            dateFrom.GetFirstDayOfYear(),
            dateFrom.GetLastDayOfYear(),
            userId,
            leaveTypeId,
            nestedLeaveTypeIds);
        if (leaveLimit.Limit != null &&
            CalculateRemaningLimit(leaveLimit.Limit.Value, leaveLimit.OverdueLimit, totalUsed + duration) < TimeSpan.Zero)
        {
            throw new ValidationException("You don't have enough free days for this type of leave");
        }
    }

    private TimeSpan CalculateRemaningLimit(TimeSpan limit, TimeSpan? overdueLimit, TimeSpan usedLimits)
    {
        return limit + (overdueLimit ?? TimeSpan.Zero) - usedLimits;
    }
}
