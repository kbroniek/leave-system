namespace LeaveSystem.Shared.LeaveRequests;

public enum LeaveRequestStatus
{
    Pending = 1,
    Accepted = 2,
    Canceled = 3,
    Rejected = 4,
}

public static class LeaveRequestStatusExtensions
{
    public static bool IsValid(this LeaveRequestStatus status)
    {
        return status == LeaveRequestStatus.Pending || status == LeaveRequestStatus.Accepted;
    }
}
