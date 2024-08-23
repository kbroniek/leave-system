namespace LeaveSystem.Shared.Dto;
using System;
using LeaveSystem.Shared.LeaveRequests;

public record GetLeaveRequestDto(Guid LeaveRequestId, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration, Guid LeaveTypeId, LeaveRequestStatus Status, string OwnerUserId, string LastModifiedById, string CreatedById, TimeSpan WorkingHours, DateTimeOffset CreatedDate, DateTimeOffset ModifiedDate, IEnumerable<GetLeaveRequestDto.RemarksDto> Remarks)
{
    public record RemarksDto(string Remarks, string CreatedById, DateTimeOffset CreatedDate);
}

public record CreateLeaveRequestDto(Guid LeaveRequestId, DateOnly DateFrom, DateOnly DateTo, Guid LeaveTypeId, TimeSpan WorkingHours, string? Remark);
public record CreateLeaveRequestOnBehalfDto(Guid LeaveRequestId, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration, Guid LeaveTypeId, TimeSpan WorkingHours, string? Remark, string OwnerUserId);
public record ChangeStatusLeaveRequestDto(string? Remark);
