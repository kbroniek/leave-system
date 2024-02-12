using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

public record LeaveRequestShortInfo(
    Guid Id,
    DateTimeOffset DateFrom,
    DateTimeOffset DateTo,
    TimeSpan Duration,
    Guid LeaveTypeId,
    LeaveRequestStatus Status,
    FederatedUser CreatedBy,
    TimeSpan WorkingHours
);
