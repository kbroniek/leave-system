namespace LeaveSystem.Shared.Dto;
using System;
using LeaveSystem.Shared.LeaveRequests;

public record SearchLeaveRequestsResultDto(Guid Id,
    DateOnly DateFrom,
    DateOnly DateTo,
    TimeSpan Duration,
    Guid LeaveTypeId,
    LeaveRequestStatus Status,
    //TODO: Return assignedTo
    LeaveRequestUserDto CreatedBy,
    TimeSpan WorkingHours);
