using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequest;

public record class LeaveRequestShortInfo(
    Guid Id,
    DateTimeOffset DateFrom,
    DateTimeOffset DateTo,
    TimeSpan Duration,
    Guid LeaveTypeId,
    LeaveRequestStatus Status,
    FederatedUser? CreatedBy
);
