using Ardalis.GuardClauses;
using GoldenEye.Backend.Core.DDD.Commands;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.Linq;
using LeaveSystem.Periods;
using LeaveSystem.Shared;
using Marten;
using MediatR;

namespace LeaveSystem.EventSourcing.WorkingHours.ModyfingWorkingHours;

public class ModifyWorkingHours : ICommand, IDateToNullablePeriod
{
    public Guid WorkingHoursId { get; }
    public string UserId { get; set; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset? DateTo { get; }
    public TimeSpan Duration { get; }
    public FederatedUser ModifiedBy { get; }

    private ModifyWorkingHours(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo,
        TimeSpan duration, FederatedUser modifiedBy)
    {
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        ModifiedBy = modifiedBy;
        WorkingHoursId = workingHoursId;
        UserId = userId;
    }

    public static ModifyWorkingHours Create(Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo, TimeSpan? duration, FederatedUser? addedBy) =>
        new(
            Guard.Against.NillAndDefault(workingHoursId),
            Guard.Against.NullOrWhiteSpace(userId),
            Guard.Against.NillAndDefault(dateFrom).GetDayWithoutTime(),
            dateTo?.GetDayWithoutTime(),
            Guard.Against.NillAndDefault(duration),
            Guard.Against.NillAndDefault(addedBy)
        );
}

internal class HandleModifyWorkingHours : ICommandHandler<ModifyWorkingHours>
{
    private readonly IRepository<WorkingHours> workingHoursRepository;
    private readonly IDocumentSession querySession;
    private readonly IRepository<LeaveRequest> leaveRequestRepository;

    public HandleModifyWorkingHours(IRepository<WorkingHours> workingHoursRepository,
        IDocumentSession querySession, IRepository<LeaveRequest> leaveRequestRepository)
    {
        this.workingHoursRepository = workingHoursRepository;
        this.querySession = querySession;
        this.leaveRequestRepository = leaveRequestRepository;
    }
    public async Task<Unit> Handle(ModifyWorkingHours request, CancellationToken cancellationToken)
    {
        var periodOverlapExp = PeriodExpressions.GetPeriodOverlapExp<WorkingHours>(request);
        var requestOverlappingOtherWorkingHours = querySession.Query<WorkingHours>()
            .Any(periodOverlapExp.And(x => x.UserId == request.UserId && x.Id != request.WorkingHoursId));
        if (requestOverlappingOtherWorkingHours)
        {
            throw new InvalidOperationException("You can't add working hours in this period, because other working hours overlap it");
        }
        var workingHours = await workingHoursRepository.FindByIdAsync(request.WorkingHoursId, cancellationToken) ??
            throw GoldenEye.Backend.Core.Exceptions.NotFoundException.For<WorkingHours>(request.WorkingHoursId);
        workingHours.Modify(request);
        await DeprecateLeaveRequests(request, cancellationToken);
        await workingHoursRepository.UpdateAsync(workingHours, cancellationToken);
        await workingHoursRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task DeprecateLeaveRequests(ModifyWorkingHours command, CancellationToken cancellationToken)
    {
        //TODO: Deprecate anly future
        var periodOverlapExp = PeriodExpressions.GetPeriodOverlapExp<LeaveRequest, ModifyWorkingHours>(command);
        var overlappingLeaveRequestsOfUser = querySession.Query<LeaveRequest>()
            .Where(periodOverlapExp.And(x => x.CreatedBy.Id == command.UserId).And(LeaveRequestExpressions.IsValidExpression));
        foreach (var leaveRequest in overlappingLeaveRequestsOfUser)
        {
            leaveRequest.Deprecate("deprecated due to changing working Hours", command.ModifiedBy);
            await leaveRequestRepository.UpdateAsync(leaveRequest, cancellationToken);
            await leaveRequestRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
