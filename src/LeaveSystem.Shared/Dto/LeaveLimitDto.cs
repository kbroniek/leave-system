namespace LeaveSystem.Shared.Dto;
using System;

public record LeaveLimitDto(Guid Id, TimeSpan? Limit, TimeSpan? OverdueLimit, TimeSpan WorkingHours, Guid LeaveTypeId,
        DateOnly? ValidSince, DateOnly? ValidUntil, string AssignedToUserId, string? Description = null);

