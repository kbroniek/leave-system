namespace LeaveSystem.Shared.WorkingHours;

public record WorkingHoursDto(string UserId, DateTimeOffset DateFrom, DateTimeOffset? DateTo, TimeSpan Duration, WorkingHoursStatus Status);