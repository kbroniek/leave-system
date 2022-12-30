﻿using Ardalis.GuardClauses;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Services;
using LeaveSystem.Shared;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LeaveRequestFactory
{
    private readonly CreateLeaveRequestValidator validator;
    private readonly WorkingHoursService workingHoursService;
    private readonly LeaveSystemDbContext dbContext;

    public LeaveRequestFactory(CreateLeaveRequestValidator validator, WorkingHoursService workingHoursService, LeaveSystemDbContext dbContext)
    {
        this.validator = validator;
        this.workingHoursService = workingHoursService;
        this.dbContext = dbContext;
    }

    public virtual async Task<LeaveRequest> Create(CreateLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveType = await GetLeaveType(command.LeaveTypeId);
        var workingHours = await workingHoursService.GetUserSingleWorkingHoursDuration(command.CreatedBy.Id, command.DateFrom, command.DateTo, cancellationToken);
        var maxDuration = DateCalculator.CalculateDuration(command.DateFrom, command.DateTo, workingHours, leaveType.Properties?.IncludeFreeDays);
        var minDuration = maxDuration - workingHours;
        var duration = command.Duration ?? maxDuration;
        var leaveRequestCreated = LeaveRequestCreated.Create(command.LeaveRequestId, command.DateFrom, command.DateTo, duration, command.LeaveTypeId, command.Remarks, command.CreatedBy);
        validator.BasicValidate(leaveRequestCreated, minDuration, maxDuration, leaveType.Properties?.IncludeFreeDays);
        await validator.ImpositionValidator(leaveRequestCreated);
        await validator.LimitValidator(leaveRequestCreated);
        return LeaveRequest.CreatePendingLeaveRequest(leaveRequestCreated);
    }

    private async Task<LeaveType> GetLeaveType(Guid leaveTypeId)
    {
        var leaveType = await dbContext.LeaveTypes.FindAsync(leaveTypeId);
        if (leaveType == null)
        {
            throw new NotFoundException(leaveTypeId.ToString(), nameof(leaveTypeId));
        }

        return leaveType;
    }
}

