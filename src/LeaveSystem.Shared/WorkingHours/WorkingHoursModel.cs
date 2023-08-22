namespace LeaveSystem.Shared.WorkingHours;

public record WorkingHoursModel(string UserId, DateTimeOffset DateFrom, DateTimeOffset DateTo, TimeSpan Duration);
