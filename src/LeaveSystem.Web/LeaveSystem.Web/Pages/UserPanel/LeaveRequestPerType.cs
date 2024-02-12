using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using static LeaveSystem.Web.Pages.LeaveTypes.LeaveTypesService;

namespace LeaveSystem.Web.Pages.UserPanel;
public record LeaveRequestPerType(string LeaveTypeName, Guid LeaveTypeId, string Used, string? Limit, string? OverdueLimit, string? SumLimit, string? Left, IEnumerable<LeaveRequestPerType.ForView> LeaveRequests, LeaveTypeProperties LeaveTypeProperties)
{
    public static LeaveRequestPerType Create(LeaveTypeDto leaveType, IEnumerable<LeaveTypeDto> allLeaveTypes, IEnumerable<LeaveRequestShortInfo> leaveRequests, IEnumerable<UserLeaveLimitDto> limits, TimeSpan workingHours)
    {
        var connectedLeaveType = allLeaveTypes.FirstOrDefault(lt => lt.BaseLeaveTypeId == leaveType.Id);
        var leaveRequestsPerLeaveType = leaveRequests
            .Where(lr => lr.LeaveTypeId == leaveType.Id || (connectedLeaveType is not null && lr.LeaveTypeId == connectedLeaveType.Id));
        var validLeaveRequestsPerLeaveType = leaveRequestsPerLeaveType
            .Where(lr => lr.Status.IsValid());
        var limitsPerLeaveType = limits
            .Where(l => l.LeaveTypeId == leaveType.Id);

        var leaveRequestsWithDescription = leaveRequestsPerLeaveType
            .Select(lr => ForView.CreateForView(lr, limitsPerLeaveType, workingHours));
        TimeSpan leaveRequestsUsed = CalculateUsed(validLeaveRequestsPerLeaveType);
        var limitsSum = TimeSpan.FromTicks(limitsPerLeaveType.Sum(lr => lr.Limit.Ticks));
        var overdueLimitSum = TimeSpan.FromTicks(limitsPerLeaveType.Sum(lr => lr.OverdueLimit.Ticks));
        var totalLimit = limitsSum + overdueLimitSum;
        var left = totalLimit - leaveRequestsUsed;
        return new LeaveRequestPerType(
            leaveType.Name,
            leaveType.Id,
            leaveRequestsUsed.GetReadableTimeSpan(workingHours),
            limitsPerLeaveType.Any() ? limitsSum.GetReadableTimeSpan(workingHours) : null,
            limitsPerLeaveType.Any() ? overdueLimitSum.GetReadableTimeSpan(workingHours) : null,
            limitsPerLeaveType.Any() ? totalLimit.GetReadableTimeSpan(workingHours) : null,
            limitsPerLeaveType.Any() ? left.GetReadableTimeSpan(workingHours) : null,
            leaveRequestsWithDescription,
            leaveType.Properties);
    }

    private static TimeSpan CalculateUsed(IEnumerable<LeaveRequestShortInfo> leaveRequests) =>
        TimeSpan.FromTicks(leaveRequests.Sum(lr => lr.Duration.Ticks));

    public record ForView(Guid Id, DateTimeOffset DateFrom, DateTimeOffset DateTo, string Duration, string? Description, LeaveRequestStatus Status)
    {
        public static ForView CreateForView(LeaveRequestShortInfo leaveRequest, IEnumerable<UserLeaveLimitDto> limits, TimeSpan workingHours)
        {
            var limit = limits.FirstOrDefault(l =>
                (l.ValidSince == null || l.ValidSince <= leaveRequest.DateFrom) &&
                (l.ValidUntil == null || l.ValidUntil >= leaveRequest.DateTo));
            return new ForView(
                leaveRequest.Id,
                leaveRequest.DateFrom,
                leaveRequest.DateTo,
                leaveRequest.Duration.GetReadableTimeSpan(workingHours),
                limit?.Property?.Description,
                leaveRequest.Status
            );
        }
    }
}
