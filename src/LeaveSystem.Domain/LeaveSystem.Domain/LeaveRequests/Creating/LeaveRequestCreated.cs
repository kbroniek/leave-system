namespace LeaveSystem.Domain.LeaveRequests.Creating;

using Ardalis.GuardClauses;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Shared.Dto;

public record LeaveRequestCreated(
    Guid LeaveRequestId,
    DateOnly DateFrom,
    DateOnly DateTo,
    TimeSpan Duration,
    Guid LeaveTypeId,
    string? Remarks,
    LeaveRequestUserDto CreatedBy,
    LeaveRequestUserDto AssignedTo,
    TimeSpan WorkingHours,
    DateTimeOffset CreatedDate
) : IEvent
{
    public Guid StreamId => LeaveRequestId;

    internal static LeaveRequestCreated Create(Guid leaveRequestId, DateOnly dateFrom, DateOnly dateTo, TimeSpan duration, Guid leaveTypeId, string? remarks, LeaveRequestUserDto createdBy, LeaveRequestUserDto assignedTo, TimeSpan workingHours, DateTimeOffset createdDate)
    {
        const int hoursInDayCount = 24;
        const int minHoursInDayCount = 1;
        Guard.Against.OutOfRange(workingHours, nameof(workingHours), TimeSpan.FromHours(minHoursInDayCount), TimeSpan.FromHours(hoursInDayCount));
        return new(leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, remarks, createdBy, assignedTo, workingHours, createdDate);
    }
}
