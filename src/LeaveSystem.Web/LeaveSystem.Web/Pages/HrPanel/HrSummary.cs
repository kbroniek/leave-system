using LeaveSystem.Shared;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;

namespace LeaveSystem.Web.Pages.HrPanel;

public record class HrSummary(
        string Employee,
        string SumLimit,
        string Limit,
        string OverdueLimit,
        string Left,
        bool usedDaysInRow,
        string OnDemand,
        string Saturdays,
        IDictionary<Guid, string> UsedPerLeaveTypes)
{
    public static HrSummary Create(
        FederatedUser user,
        IEnumerable<LeaveTypesService.LeaveTypeDto> leaveTypes,
        IEnumerable<LeaveRequestShortInfo> leaveRequests,
        IEnumerable<UserLeaveLimitsService.LeaveLimitDto> limits,
        TimeSpan workingHours,
        int daysInARow)
    {
        var userLeaveRequests = leaveRequests
            .Where(lr => string.Equals(lr.CreatedBy.Id, user.Id, StringComparison.CurrentCultureIgnoreCase))
            .ToList();
        var userLimits = limits
            .Where(l => string.Equals(l.AssignedToUserId, user.Id, StringComparison.CurrentCultureIgnoreCase))
            .ToList();
        var usedPerLeaveTypes = leaveTypes.ToDictionary(
            lt => lt.Id,
            lt => CalculateUsedLimit(lt.Id, userLeaveRequests));
        var holidayLeaveType = GetLeaveTypeOrDefault(leaveTypes, LeaveTypeCatalog.Holiday);
        var onDemandLeaveType = GetLeaveTypeOrDefault(leaveTypes, LeaveTypeCatalog.OnDemand);
        var holidayLimit = GetLimitOrDefault(holidayLeaveType?.Id, userLimits);
        var totalLimit = holidayLimit?.TotalLimit.GetReadableTimeSpan(workingHours) ?? "";
        var limit = holidayLimit?.Limit.GetReadableTimeSpan(workingHours) ?? "";
        var overdueLimit = holidayLimit?.OverdueLimit.GetReadableTimeSpan(workingHours) ?? "";
        var holidayUsedLeaveType = CalculateLimitLeft(holidayLimit, leaveTypes, usedPerLeaveTypes, workingHours);
        var usedDaysInARow = holidayLeaveType == null ? false : CalculateDaysInARow(
            userLeaveRequests,
            holidayLeaveType.Id,
            daysInARow);
        var onDemandDescription = CalculateUsedAndTotalShortInfo(onDemandLeaveType?.Id, usedPerLeaveTypes, userLimits, workingHours);
        var saturdayLeaveType = GetLeaveTypeOrDefault(leaveTypes, LeaveTypeCatalog.Saturday);
        var saturdayDescription = CalculateUsedAndTotalShortInfo(saturdayLeaveType?.Id, usedPerLeaveTypes, userLimits, workingHours);
        var usedPerLeaveTypesReadable = usedPerLeaveTypes.ToDictionary(
            lt => lt.Key,
            lt => lt.Value.GetReadableTimeSpan(workingHours));
        return new HrSummary(
            user.Name ?? user.Email ?? user.Id,
            totalLimit,
            limit,
            overdueLimit,
            holidayUsedLeaveType,
            usedDaysInARow,
            onDemandDescription,
            saturdayDescription,
            usedPerLeaveTypesReadable
        );
    }

    private static string CalculateLimitLeft(
        UserLeaveLimitsService.LeaveLimitDto? holidayLimit,
        IEnumerable<LeaveTypesService.LeaveTypeDto> leaveTypes,
        IDictionary<Guid, TimeSpan> usedPerLeaveTypes,
        TimeSpan workingHours)
    {
        if (holidayLimit == null)
        {
            return "";
        }
        usedPerLeaveTypes.TryGetValue(holidayLimit.LeaveTypeId, out TimeSpan usedHoliday);
        TimeSpan usedConnected = TimeSpan.Zero;
        foreach (var leaveType in leaveTypes.Where(lt => lt.BaseLeaveTypeId == holidayLimit.LeaveTypeId))
        {
            usedPerLeaveTypes.TryGetValue(leaveType?.Id ?? Guid.Empty, out TimeSpan temp);
            usedConnected += temp;
        }
        var left = holidayLimit.TotalLimit - usedHoliday - usedConnected;
        return left.GetReadableTimeSpan(workingHours);
    }

    private static LeaveTypesService.LeaveTypeDto? GetLeaveTypeOrDefault(
        IEnumerable<LeaveTypesService.LeaveTypeDto> leaveTypes,
        LeaveTypeCatalog leaveTypeCatalog)
    {
        return leaveTypes.FirstOrDefault(lt => lt.Properties.Catalog == leaveTypeCatalog);
    }

    private static string CalculateUsedAndTotalShortInfo(
        Guid? leaveTypeId,
        IDictionary<Guid, TimeSpan> usedPerLeaveTypes,
        IEnumerable<UserLeaveLimitsService.LeaveLimitDto> limits,
        TimeSpan workingHours)
    {
        if (leaveTypeId == null)
        {
            return "";
        }
        usedPerLeaveTypes.TryGetValue(leaveTypeId.Value, out TimeSpan leaveRequestsDuration);
        var totalLimit = TimeSpan.FromTicks(limits
            .Where(l => l.LeaveTypeId == leaveTypeId)
            .Sum(l => l.TotalLimit.Ticks));
        var leaveRequestsDurationReadable = leaveRequestsDuration == TimeSpan.Zero ? "0d" : leaveRequestsDuration.GetReadableTimeSpan(workingHours);
        var totalLimitReadable = totalLimit.GetReadableTimeSpan(workingHours);
        return $"{leaveRequestsDurationReadable} / {totalLimitReadable}";
    }

    private static bool CalculateDaysInARow(
        IEnumerable<LeaveRequestShortInfo> leaveRequests,
        Guid leaveTypeId,
        int daysInARow)
    {
        return leaveRequests.Any(lr =>
            lr.LeaveTypeId == leaveTypeId &&
            DateCalculator.CalculateDurationIncludeFreeDays(lr.DateFrom.GetDayWithoutTime(), lr.DateTo.GetDayWithoutTime()) >= daysInARow
        );
    }

    private static UserLeaveLimitsService.LeaveLimitDto? GetLimitOrDefault(
        Guid? leaveTypeId,
        IEnumerable<UserLeaveLimitsService.LeaveLimitDto> limits)
    {
        return leaveTypeId == null ? null :
            limits.FirstOrDefault(l => l.LeaveTypeId == leaveTypeId.Value);
    }

    private static TimeSpan CalculateUsedLimit(
        Guid leaveTypeId,
        IEnumerable<LeaveRequestShortInfo> leaveRequests)
    {
        return TimeSpan.FromTicks(leaveRequests
            .Where(lr => lr.LeaveTypeId == leaveTypeId)
            .Sum(lr => lr.Duration.Ticks));
    }
};