namespace LeaveSystem.Domain.LeaveRequests;

using Ardalis.GuardClauses;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared;

public record LeaveRequestCreated(
    Guid LeaveRequestId,
    DateOnly DateFrom,
    DateOnly DateTo,
    TimeSpan Duration,
    Guid LeaveTypeId,
    string? Remarks,
    FederatedUser CreatedBy,
    TimeSpan WorkingHours
) : IEvent
{
    public Guid StreamId => LeaveRequestId;

    public static LeaveRequestCreated Create(Guid leaveRequestId, DateOnly dateFrom, DateOnly dateTo, TimeSpan duration, Guid leaveTypeId, string? remarks, FederatedUser createdBy, TimeSpan workingHours)
    {
        const int hoursInDayCount = 24;
        const int minHoursInDayCount = 1;
        Guard.Against.OutOfRange(workingHours, nameof(workingHours), TimeSpan.FromHours(minHoursInDayCount), TimeSpan.FromHours(hoursInDayCount));
        return new(leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, remarks, createdBy, workingHours);
    }
}
