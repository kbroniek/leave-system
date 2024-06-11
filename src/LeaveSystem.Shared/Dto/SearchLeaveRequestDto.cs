namespace LeaveSystem.Shared.Dto;
using System;
using LeaveSystem.Shared.LeaveRequests;

public record SearchLeaveRequestDto(Guid Id,
    DateOnly DateFrom,
    DateOnly DateTo,
    TimeSpan Duration,
    Guid LeaveTypeId,
    LeaveRequestStatus Status,
    string CreatedById,
    TimeSpan WorkingHours);
