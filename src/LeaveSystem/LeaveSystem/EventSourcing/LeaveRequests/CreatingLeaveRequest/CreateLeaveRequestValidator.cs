﻿using Ardalis.GuardClauses;
using Baseline;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Services;
using Marten;
using System.ComponentModel.DataAnnotations;
using EFExtensions = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
public class CreateLeaveRequestValidator
{
    private const string LeaveTypeName = "leave_request_created";
    private readonly LeaveSystemDbContext dbContext;
    private readonly WorkingHoursService workingHoursService;
    private readonly IDocumentSession documentSession;

    public CreateLeaveRequestValidator(LeaveSystemDbContext dbContext, WorkingHoursService workingHoursService, IDocumentSession documentSession)
    {
        this.dbContext = dbContext;
        this.workingHoursService = workingHoursService;
        this.documentSession = documentSession;
    }

    public virtual void BasicValidate(LeaveRequest creatingLeaveRequest, TimeSpan minDuration, TimeSpan maxDuration, bool? includeFreeDays)
    {
        Guard.Against.OutOfRange(creatingLeaveRequest.Duration, nameof(creatingLeaveRequest.Duration), minDuration, maxDuration);
        if (includeFreeDays == false)
        {
            var dateFromDayKind = workingHoursService.getDayKind(creatingLeaveRequest.DateFrom);
            if (dateFromDayKind != WorkingHoursService.DayKind.WORKING)
            {
                throw new ArgumentOutOfRangeException(nameof(creatingLeaveRequest.DateFrom), "The date is off work.");
            }
            var dateToDayKind = workingHoursService.getDayKind(creatingLeaveRequest.DateTo);
            if (dateToDayKind != WorkingHoursService.DayKind.WORKING)
            {
                throw new ArgumentOutOfRangeException(nameof(creatingLeaveRequest.DateTo), "The date is off work.");
            }
        }
    }

    public virtual async Task ImpositionValidator(LeaveRequest creatingLeaveRequest)
    {
        var leaveRequestCreatedEvents = await documentSession.Events.QueryAllRawEvents()
            .Where(x => x.EventTypeName == LeaveTypeName &&
                x.As<LeaveRequestCreated>().CreatedBy.Email == creatingLeaveRequest.CreatedBy.Email && (
                    x.As<LeaveRequestCreated>().DateFrom >= creatingLeaveRequest.DateTo &&
                    x.As<LeaveRequestCreated>().DateTo <= creatingLeaveRequest.DateTo
                ) || (
                    x.As<LeaveRequestCreated>().DateFrom >= creatingLeaveRequest.DateFrom &&
                    x.As<LeaveRequestCreated>().DateTo <= creatingLeaveRequest.DateFrom
                ) || (
                    x.As<LeaveRequestCreated>().DateFrom >= creatingLeaveRequest.DateFrom &&
                    x.As<LeaveRequestCreated>().DateTo <= creatingLeaveRequest.DateTo
                ) || (
                    x.As<LeaveRequestCreated>().DateFrom <= creatingLeaveRequest.DateFrom &&
                    x.As<LeaveRequestCreated>().DateTo >= creatingLeaveRequest.DateTo
                ))
            .ToListAsync();

        foreach (var @event in leaveRequestCreatedEvents)
        {
            var leaveRequestFromDb = await documentSession.Events.AggregateStreamAsync<LeaveRequest>(@event.StreamId);
            if ((leaveRequestFromDb?.Status & LeaveRequestStatus.Valid) > 0)
            {
                throw new ValidationException("Cannot create a new leave request in this time. The other leave is overlapping with this date.");
            }
        }
    }

    public virtual async Task LimitValidator(LeaveRequest creatingLeaveRequest)
    {
        var connectedLeaveTypeIds = await GetConnectedLeaveTypeIds(creatingLeaveRequest.LeaveTypeId);
        await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
            creatingLeaveRequest.DateTo,
            creatingLeaveRequest.LeaveTypeId,
            creatingLeaveRequest.CreatedBy.Email,
            creatingLeaveRequest.LeaveTypeId,
            connectedLeaveTypeIds.nestedLeaveTypeId);

        if (connectedLeaveTypeIds.baseLeaveTypeId != null)
        {
            var baseLeaveTypeId = connectedLeaveTypeIds.baseLeaveTypeId.Value;
            await CheckLimitForBaseLeave(creatingLeaveRequest.DateFrom,
                creatingLeaveRequest.DateTo,
                baseLeaveTypeId,
                creatingLeaveRequest.CreatedBy.Email,
                baseLeaveTypeId);
        }
    }

    private async Task CheckLimitForBaseLeave(
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        Guid currentLeaveTypeId,
        string userEmail,
        Guid leaveTypeId,
        Guid? nestedLeaveTypeId = null)
    {
        UserLeaveLimit leaveLimit = await GetLimits(dateFrom,
               dateTo,
               currentLeaveTypeId,
               userEmail);
        var totalUsed = await GetUsedLeavesDuration(dateFrom.Year,
            userEmail,
            leaveTypeId,
            nestedLeaveTypeId);
        if (CalculateRemaningLimit(leaveLimit.Limit, leaveLimit.OverdueLimit, totalUsed) <= TimeSpan.Zero)
        {
            throw new ValidationException("You don't have enough free days for this type of leave");
        }
    }

    private TimeSpan CalculateRemaningLimit(TimeSpan limit, TimeSpan? overdueLimit, TimeSpan usedLimits)
    {
        return limit + (overdueLimit ?? TimeSpan.Zero) - usedLimits;
    }

    private async Task<(Guid? baseLeaveTypeId, Guid? nestedLeaveTypeId)> GetConnectedLeaveTypeIds(Guid leaveTypeId)
    {
        var leaveTypes = await EFExtensions.ToListAsync(dbContext.LeaveTypes.Where(l =>
            l.BaseLeaveTypeId == leaveTypeId || l.LeaveTypeId == leaveTypeId));
        if (leaveTypes.Count == 0)
        {
            return (null, null);
        }
        var nestedLeaveType = leaveTypes.FirstOrDefault(l => l.BaseLeaveTypeId == leaveTypeId);
        var currentLeaveType = leaveTypes.FirstOrDefault(l => l.LeaveTypeId == leaveTypeId);
        return (currentLeaveType?.BaseLeaveTypeId, nestedLeaveType?.LeaveTypeId);
    }

    private async Task<TimeSpan> GetUsedLeavesDuration(
        int year,
        string userEmail,
        Guid leaveTypeId,
        Guid? nestedLeaveTypeId)
    {
        var firstDay = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var lastDay = new DateTimeOffset(year, 12, 31, 23, 59, 59, 999, TimeSpan.Zero);
        var command = documentSession.Events.QueryAllRawEvents()
            .Where(x => x.EventTypeName == LeaveTypeName &&
                x.As<LeaveRequestCreated>().CreatedBy.Email == userEmail &&
                x.As<LeaveRequestCreated>().DateFrom >= firstDay &&
                x.As<LeaveRequestCreated>().DateTo <= lastDay &&
                (
                    x.As<LeaveRequestCreated>().LeaveTypeId == leaveTypeId ||
                    (nestedLeaveTypeId != null && x.As<LeaveRequestCreated>().LeaveTypeId == nestedLeaveTypeId)
                )
             ).ToCommand();
        var leaveRequestCreatedEvents = await documentSession.Events.QueryAllRawEvents()
            .Where(x => x.EventTypeName == LeaveTypeName &&
                x.As<LeaveRequestCreated>().CreatedBy.Email == userEmail &&
                x.As<LeaveRequestCreated>().DateFrom >= firstDay &&
                x.As<LeaveRequestCreated>().DateTo <= lastDay &&
                (
                    x.As<LeaveRequestCreated>().LeaveTypeId == leaveTypeId ||
                    (nestedLeaveTypeId != null && x.As<LeaveRequestCreated>().LeaveTypeId == nestedLeaveTypeId)
                )
             ).ToListAsync();

        TimeSpan usedLimits = TimeSpan.Zero;
        foreach (var @event in leaveRequestCreatedEvents)
        {
            var leaveRequestFromDb = await documentSession.Events.AggregateStreamAsync<LeaveRequest>(@event.StreamId);
            if (leaveRequestFromDb != null && (leaveRequestFromDb.Status & LeaveRequestStatus.Valid) > 0)
            {
                usedLimits += leaveRequestFromDb.Duration;
            }
        }
        return usedLimits;
    }

    private async Task<UserLeaveLimit> GetLimits(
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        Guid leaveTypeId,
        string userEmail)
    {
        var limits = await EFExtensions.ToListAsync(dbContext.UserLeaveLimits.Where(l =>
                        (l.User == null || l.User.Email == userEmail) &&
                        (l.ValidSince == null || l.ValidSince <= dateFrom) &&
                        (l.ValidUntil == null || l.ValidUntil >= dateTo) &&
                        l.LeaveTypeId == leaveTypeId));
        if (limits == null || limits.Count == 0)
        {
            throw new ValidationException($"Cannot find limits for the leave type id: {leaveTypeId}. User {userEmail}.");
        }
        if (limits.Count > 1)
        {
            throw new ValidationException($"Two or more limits found which are the same for the leave type id: {leaveTypeId}. User {userEmail}.");
        }
        return limits.First();
    }
}
