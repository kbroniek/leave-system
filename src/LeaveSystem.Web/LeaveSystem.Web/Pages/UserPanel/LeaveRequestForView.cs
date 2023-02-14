using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;

namespace LeaveSystem.Web.Pages.UserPanel;

public record class LeaveRequestForView(DateTimeOffset DateFrom, DateTimeOffset DateTo, string Duration, LeaveRequestStatus Status, string LeaveTypeName)
{
    public static LeaveRequestForView Create(LeaveRequestShortInfo leaveRequest, IEnumerable<LeaveTypesService.LeaveTypeDto> leaveTypes, TimeSpan workingHours)
    {
        return new LeaveRequestForView(
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.Duration.GetReadableTimeSpan(workingHours),
            leaveRequest.Status,
            leaveTypes.FirstOrDefault(lt => lt.Id == leaveRequest.LeaveTypeId)?.Name ?? leaveRequest.LeaveTypeId.ToString()
        );
    }
}
