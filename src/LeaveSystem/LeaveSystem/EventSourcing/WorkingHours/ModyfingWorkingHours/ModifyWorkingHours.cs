using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Extensions;
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

    public static ModifyWorkingHours Create(Guid? workingHoursId, string userId, DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo, TimeSpan? duration, FederatedUser? addedBy) =>
        new(
            Guard.Against.NillAndDefault(workingHoursId),
            Guard.Against.NullOrWhiteSpace(userId),
            Guard.Against.NillAndDefault(dateFrom),
            dateTo,
            Guard.Against.NillAndDefault(duration),
            Guard.Against.NillAndDefault(addedBy)
        );
}

internal class HandleModifyWorkingHours : ICommandHandler<ModifyWorkingHours>
{
    private readonly IRepository<WorkingHours> workingHoursRepository;
    private readonly WorkingHoursFactory factory;
    private readonly IDocumentSession querySession;
    private readonly IRepository<LeaveRequest> leaveRequestRepository;

    public HandleModifyWorkingHours(IRepository<WorkingHours> workingHoursRepository, WorkingHoursFactory factory,
        IDocumentSession querySession, IRepository<LeaveRequest> leaveRequestRepository)
    {
        this.workingHoursRepository = workingHoursRepository;
        this.factory = factory;
        this.querySession = querySession;
        this.leaveRequestRepository = leaveRequestRepository;
    }

    //Todo: Add deprecating leave requests
    public async Task<Unit> Handle(ModifyWorkingHours request, CancellationToken cancellationToken)
    {
        var requestOverlappingOtherWorkingHours = querySession.Query<WorkingHours>()
            .Any(x => x.UserId == request.UserId && request.PeriodsOverlaps(x));
        if (requestOverlappingOtherWorkingHours)
        {
            throw new InvalidOperationException("You cant add working hours in this period, because other");
        }
        var workingHours = await workingHoursRepository.FindById(request.WorkingHoursId, cancellationToken);
        workingHours.Modify(request);
        await workingHoursRepository.Update(workingHours, cancellationToken);
        await workingHoursRepository.SaveChanges(cancellationToken);
        return Unit.Value;
    }

    private async Task DeprecateLeaveRequests()
    {
        
    }
}