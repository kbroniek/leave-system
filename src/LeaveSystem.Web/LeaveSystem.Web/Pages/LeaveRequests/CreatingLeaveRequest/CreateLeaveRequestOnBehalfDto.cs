using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

public record class CreateLeaveRequestOnBehalfDto : CreateLeaveRequestDto
{
    public FederatedUser? CreatedByOnBehalf { get; set; }
}
