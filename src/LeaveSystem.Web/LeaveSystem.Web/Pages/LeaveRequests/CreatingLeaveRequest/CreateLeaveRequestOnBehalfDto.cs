using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequestOnBehalfDto : CreateLeaveRequestDto
{
    public FederatedUser? CreatedBy { get; set; }
}
