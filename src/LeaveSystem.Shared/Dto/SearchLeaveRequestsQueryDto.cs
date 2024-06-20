using LeaveSystem.Shared.LeaveRequests;

namespace LeaveSystem.Shared.Dto;

public record SearchLeaveRequestsQueryDto(int? PageNumber, int? PageSize, DateOnly? DateFrom, DateOnly? DateTo,
    Guid[]? LeaveTypeIds, LeaveRequestStatus[]? Statuses, string[]? CreatedByEmails, string[]? CreatedByUserIds);