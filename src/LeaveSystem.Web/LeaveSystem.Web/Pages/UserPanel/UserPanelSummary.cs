using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using static LeaveSystem.Web.Pages.UserLeaveLimits.UserLeaveLimitsService;

namespace LeaveSystem.Web.Pages.UserPanel;
public record class LeaveRequestPerType(string LeaveType, string Used, string? Limit, string? OverdueLimit, string? SumLimit, string? Left, IEnumerable<LeaveRequestPerType.ForView> LeaveRequests)
{
    public static LeaveRequestPerType Create(LeaveTypesService.LeaveTypeDto leaveType, IEnumerable<LeaveRequestShortInfo> leaveRequests, IEnumerable<UserLeaveLimitDto> limits, TimeSpan workingHours)
    {
        var leaveRequestsPerLeaveType = leaveRequests
            .Where(lr => lr.LeaveTypeId == leaveType.Id);
        var validLeaveRequestsPerLeaveType = leaveRequestsPerLeaveType
            .Where(lr => lr.Status.IsValid());
        var limitsPerLeaveType = limits
            .Where(l => l.LeaveTypeId == leaveType.Id);

        var leaveRequestsWithDescription = leaveRequestsPerLeaveType
            .Select(lr => ForView.Create(lr, limitsPerLeaveType, workingHours));
        var leaveRequestsUsed = TimeSpan.FromTicks(validLeaveRequestsPerLeaveType.Sum(lr => lr.Duration.Ticks));
        var limitsSum = TimeSpan.FromTicks(limitsPerLeaveType.Sum(lr => lr.Limit.Ticks));
        var overdueLimitSum = TimeSpan.FromTicks(limitsPerLeaveType.Sum(lr => lr.OverdueLimit.Ticks));
        var limitTotal = limitsSum + overdueLimitSum;
        var left = limitTotal - leaveRequestsUsed;
        return new LeaveRequestPerType(
            leaveType.Name,
            leaveRequestsUsed.GetReadableTimeSpan(workingHours),
            limitsPerLeaveType.Any() ? limitsSum.GetReadableTimeSpan(workingHours) : null,
            limitsPerLeaveType.Any() ? overdueLimitSum.GetReadableTimeSpan(workingHours) : null,
            limitsPerLeaveType.Any() ? limitTotal.GetReadableTimeSpan(workingHours) : null,
            limitsPerLeaveType.Any() ? left.GetReadableTimeSpan(workingHours) : null,
            leaveRequestsWithDescription);
    }
    public record class ForView(DateTimeOffset DateFrom, DateTimeOffset DateTo, string Duration, string? Description, LeaveRequestStatus Status)
    {
        public static ForView Create(LeaveRequestShortInfo leaveRequest, IEnumerable<UserLeaveLimitDto> limits, TimeSpan workingHours)
        {
            var limit = limits.FirstOrDefault(l =>
                (l.ValidSince == null || l.ValidSince <= leaveRequest.DateFrom) &&
                (l.ValidUntil == null || l.ValidUntil >= leaveRequest.DateTo));
            return new ForView(
                leaveRequest.DateFrom,
                leaveRequest.DateTo,
                leaveRequest.Duration.GetReadableTimeSpan(workingHours),
                limit?.Property?.Description,
                leaveRequest.Status
            );
        }
    }
};
