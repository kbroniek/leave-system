namespace LeaveSystem.Shared.Dto;
using System;
using LeaveSystem.Shared.LeaveRequests;

public record SearchLeaveRequestDto(Guid Id,
                                    DateTimeOffset DateFrom,
                                    DateTimeOffset DateTo,
                                    TimeSpan Duration,
                                    LeaveRequestStatus Status,
                                    Guid CreatedBy,
                                    TimeSpan WorkingHours,
                                    SearchLeaveRequestDto.LeaveTypeDto LeaveType)
{
    public record LeaveTypeDto(Guid LeaveTypeId, string Name, string Color);
}
