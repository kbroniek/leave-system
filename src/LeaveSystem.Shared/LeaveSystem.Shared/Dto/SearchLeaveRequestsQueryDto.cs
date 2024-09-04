namespace LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;

public record SearchLeaveRequestsQueryDto(string? ContinuationToken, DateOnly? DateFrom, DateOnly? DateTo,
    Guid[]? LeaveTypeIds, LeaveRequestStatus[]? Statuses, string[]? AssignedToUserIds);
