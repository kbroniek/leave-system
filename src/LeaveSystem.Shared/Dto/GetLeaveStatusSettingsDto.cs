namespace LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;

public record GetLeaveStatusSettingsDto(LeaveRequestStatus LeaveRequestStatus, string Color);
