namespace LeaveSystem.Shared.Dto;
using System;

public record LeaveLimitDto(Guid Id, TimeSpan? Limit, TimeSpan? OverdueLimit, TimeSpan WorkingHours, Guid LeaveTypeId,
        DateOnly? ValidSince, DateOnly? ValidUntil, string AssignedToUserId, string? Description = null);


public record SearchLeaveLimitQuery(int Year, string[] UserIds, Guid[] LeaveTypeIds, int? PageSize, string? ContinuationToken);

public record SearchUserLeaveLimitQuery(int Year, Guid[] LeaveTypeIds, int? PageSize, string? ContinuationToken);
