namespace LeaveSystem.EventSourcing.LeaveRequests;

[Flags]
public enum LeaveRequestStatus
{
    Pending = 1 << 0,
    Accepted = 1 << 1,
    Printed = 1 << 2,
    Canceled = 1 << 3,
    Rejected = 1 << 4,
    Valid = Pending | Accepted | Printed,
}
