namespace LeaveSystem.Domain.LeaveLimits;

/// <summary>
/// DTO representing a leave request event for calculating used leave durations
/// </summary>
public record LeaveRequestEventDto(
    Guid StreamId,
    string AssignedToUserId,
    Guid LeaveTypeId,
    DateOnly DateFrom,
    DateOnly DateTo,
    TimeSpan Duration);
