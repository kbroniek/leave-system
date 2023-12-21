using Ardalis.GuardClauses;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;
using LeaveSystem.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Date;
using Marten;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LeaveRequestFactory
{
    private readonly CreateLeaveRequestValidator validator;
    private readonly LeaveSystemDbContext dbContext;
    private readonly IQuerySession querySession;
    private readonly CurrentDateService dateService;

    public LeaveRequestFactory(CreateLeaveRequestValidator validator, LeaveSystemDbContext dbContext,
        IQuerySession querySession, CurrentDateService dateService)
    {
        this.validator = validator;
        this.dbContext = dbContext;
        this.querySession = querySession;
        this.dateService = dateService;
    }

    public virtual async Task<LeaveRequest> Create(CreateLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveType = await GetLeaveType(command.LeaveTypeId);
        var now = dateService.GetWithoutTime();
        var workingHoursModel =
           await querySession.GetCurrentWorkingHoursForUser(command.CreatedBy.Id, now, cancellationToken)
           ?? throw new InvalidOperationException($"User with ID {command.CreatedBy.Id} does not have working Hours");
        var workingHours = workingHoursModel.Duration;
        var maxDuration = DateCalculator.CalculateDuration(command.DateFrom, command.DateTo, workingHours,
            leaveType.Properties?.IncludeFreeDays);
        var minDuration = maxDuration - workingHours + TimeSpan.FromHours(1);
        var duration = command.Duration ?? maxDuration;
        var leaveRequestCreated = LeaveRequestCreated.Create(command.LeaveRequestId, command.DateFrom, command.DateTo,
            duration, command.LeaveTypeId, command.Remarks, command.CreatedBy, workingHours);

        await validator.Validate(leaveRequestCreated, minDuration, maxDuration, leaveType.Properties?.IncludeFreeDays);

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