namespace LeaveSystem.EventSourcing.LeaveRequests;

[Flags]
public enum LeaveRequestStatus
{
    Pending = 1 << 0,
    Approved = 1 << 1,
    Canceled = 1 << 2,
    Rejected = 1 << 3,
    Valid = Pending | Approved,
}
