using System;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.TestHelpers;

internal class LeaveRequestHelper
{
    public LeaveRequest CreateLeaveRequestFromParams(
        Guid leaveRequestId, 
        DateTimeOffset dateFrom, 
        DateTimeOffset dateTo, 
        TimeSpan duration, 
        Guid leaveTypeId, 
        string? remarks, 
        FederatedUser createdBy)
    {
        var @event = LeaveRequestCreated.Create(leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, remarks, createdBy);
        return LeaveRequest.CreatePendingLeaveRequest(@event);
    } 
}