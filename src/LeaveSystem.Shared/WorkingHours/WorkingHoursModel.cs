namespace LeaveSystem.Shared.WorkingHours;

public record class WorkingHoursModel(string UserEmail, DateTimeOffset DateFrom, DateTimeOffset DateTo, TimeSpan Duration);
