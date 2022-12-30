namespace LeaveSystem.Shared.WorkingHours;

public record class WorkingHoursModel(string UserId, DateTimeOffset DateFrom, DateTimeOffset DateTo, TimeSpan Duration);
