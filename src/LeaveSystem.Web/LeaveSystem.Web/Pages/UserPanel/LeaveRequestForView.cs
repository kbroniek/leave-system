using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using static LeaveSystem.Web.Pages.LeaveTypes.LeaveTypesService;

namespace LeaveSystem.Web.Pages.UserPanel;

public record LeaveRequestForView(Guid Id, DateTimeOffset DateFrom, DateTimeOffset DateTo, string Duration, LeaveRequestStatus Status, string LeaveTypeName, LeaveTypeProperties LeaveTypeProperties, Guid LeaveTypeId, FederatedUser CreatedBy)
{
    private static readonly LeaveTypeProperties EmptyLeaveTypeProperties = new(null, null, null);
    public static LeaveRequestForView Create(LeaveRequestShortInfo leaveRequest, IEnumerable<LeaveTypeDto> leaveTypes)
    {
        var leaveTypeDto = leaveTypes.FirstOrDefault(lt => lt.Id == leaveRequest.LeaveTypeId);
        return new LeaveRequestForView(
            leaveRequest.Id,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.Duration.GetReadableTimeSpan(leaveRequest.WorkingHours),
            leaveRequest.Status,
            leaveTypeDto?.Name ?? leaveRequest.LeaveTypeId.ToString(),
            leaveTypeDto?.Properties ?? EmptyLeaveTypeProperties,
            leaveRequest.LeaveTypeId,
            leaveRequest.CreatedBy
        );
    }
}
