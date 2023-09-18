namespace LeaveSystem.Shared.WorkingHours;

public record ModifyWorkingHoursDto(string UsedId, DateTimeOffset DateFrom, DateTimeOffset? DateTo, TimeSpan Duration,
    FederatedUser AddedBy);
