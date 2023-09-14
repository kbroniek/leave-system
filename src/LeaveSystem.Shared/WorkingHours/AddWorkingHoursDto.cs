namespace LeaveSystem.Shared.WorkingHours;

public record AddWorkingHoursDto(string UserId, DateTimeOffset DateFrom, DateTimeOffset? DateTo, TimeSpan Duration, FederatedUser AddedBy);