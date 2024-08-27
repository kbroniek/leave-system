namespace LeaveSystem.Shared.WorkingHours;

public record ModifyWorkingHoursDto(string UserId, DateTimeOffset DateFrom, DateTimeOffset? DateTo, TimeSpan Duration);
