using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using static LeaveSystem.Web.Pages.LeaveTypes.LeaveTypesService;

namespace LeaveSystem.Web.Pages.UserPanel;

public record class LeaveRequestForView(Guid Id, DateTimeOffset DateFrom, DateTimeOffset DateTo, string Duration, LeaveRequestStatus Status, string LeaveTypeName, LeaveTypeProperties LeaveTypeProperties, Guid LeaveTypeId, FederatedUser CreatedBy)
{
    private static readonly LeaveTypeProperties emptyLeaveTypeProperties = new LeaveTypeProperties(null, null, null);
    public static LeaveRequestForView Create(LeaveRequestShortInfo leaveRequest, IEnumerable<LeaveTypesService.LeaveTypeDto> leaveTypes, TimeSpan workingHours)
    {
        var leaveTypeDto = leaveTypes.FirstOrDefault(lt => lt.Id == leaveRequest.LeaveTypeId);
        return new LeaveRequestForView(
            leaveRequest.Id,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            leaveRequest.Duration.GetReadableTimeSpan(workingHours),
            leaveRequest.Status,
            leaveTypeDto?.Name ?? leaveRequest.LeaveTypeId.ToString(),
            leaveTypeDto?.Properties ?? emptyLeaveTypeProperties,
            leaveRequest.LeaveTypeId,
            leaveRequest.CreatedBy
        );
    }
}
