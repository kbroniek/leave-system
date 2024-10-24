namespace LeaveSystem.Shared.LeaveRequests;
using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LeaveRequestStatus
{
    Init = 0,
    Pending = 1,
    Accepted = 2,
    Canceled = 3,
    Rejected = 4,
    Deprecated = 5
}

public static class LeaveRequestStatusExtensions
{
    public static bool IsValid(this LeaveRequestStatus status) =>
        status is LeaveRequestStatus.Pending or LeaveRequestStatus.Accepted;
}
