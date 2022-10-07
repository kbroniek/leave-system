using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests;

public record class LeaveRequestShortInfo(
    Guid Id,
    DateTimeOffset DateFrom,
    DateTimeOffset DateTo,
    TimeSpan Duration,
    Guid LeaveTypeId,
    LeaveRequestStatus Status,
    FederatedUser? CreatedBy
);
