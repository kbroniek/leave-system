using Ardalis.GuardClauses;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using Marten;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LeaveRequestFactory
{
    private readonly CreateLeaveRequestValidator validator;
    private readonly LeaveSystemDbContext dbContext;
    private readonly IQuerySession querySession;

    public LeaveRequestFactory(CreateLeaveRequestValidator validator, LeaveSystemDbContext dbContext, IQuerySession querySession)
    {
        this.validator = validator;
        this.dbContext = dbContext;
        this.querySession = querySession;
    }

    public virtual async Task<LeaveRequest> Create(CreateLeaveRequest command, CancellationToken cancellationToken)
    {
        var leaveType = await GetLeaveType(command.LeaveTypeId);
        var workingHoursModel = await querySession.Query<WorkingHours.WorkingHours>().Where(wh => wh.UserId == command.CreatedBy.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var workingHours = workingHoursModel?.Duration ?? TimeSpan.Zero;
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

