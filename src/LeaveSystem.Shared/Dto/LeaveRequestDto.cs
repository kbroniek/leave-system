namespace LeaveSystem.Shared.Dto;
using System;
using LeaveSystem.Shared.LeaveRequests;

public record GetLeaveRequestDto(Guid LeaveRequestId, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration, Guid LeaveTypeId, LeaveRequestStatus Status, FederatedUser AssignedTo, FederatedUser LastModifiedBy, FederatedUser CreatedBy, TimeSpan WorkingHours, DateTimeOffset CreatedDate, DateTimeOffset LastModifiedDate, IEnumerable<GetLeaveRequestDto.RemarksDto> Remarks)
{
    public record RemarksDto(string Remarks, FederatedUser CreatedBy, DateTimeOffset CreatedDate);
}

public record CreateLeaveRequestDto(Guid LeaveRequestId, DateOnly DateFrom, DateOnly DateTo, Guid LeaveTypeId, TimeSpan WorkingHours, string? Remark);
public record CreateLeaveRequestOnBehalfDto(Guid LeaveRequestId, DateOnly DateFrom, DateOnly DateTo, TimeSpan Duration, Guid LeaveTypeId, TimeSpan WorkingHours, string? Remark, string AssignedToId);
public record ChangeStatusLeaveRequestDto(string? Remark);
