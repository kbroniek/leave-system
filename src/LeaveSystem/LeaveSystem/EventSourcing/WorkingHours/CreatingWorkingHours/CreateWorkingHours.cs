using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Extensions;
using LeaveSystem.Linq;
using LeaveSystem.Periods;
using LeaveSystem.Shared;
using Marten;
using MediatR;

namespace LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;

public class CreateWorkingHours : ICommand, IDateToNullablePeriod
{
    public Guid WorkingHoursId { get; }
    public string UserId { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset? DateTo { get; }
    public TimeSpan Duration { get; }
    public FederatedUser CreatedBy { get; }

    private CreateWorkingHours(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo,
        TimeSpan duration, FederatedUser createdBy)
    {
        UserId = userId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        CreatedBy = createdBy;
        WorkingHoursId = workingHoursId;
    }

    public static CreateWorkingHours Create(Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo, TimeSpan? duration, FederatedUser? createdBy) =>
        new(
            Guard.Against.NillAndDefault(workingHoursId),
            Guard.Against.NullOrWhiteSpace(userId),
            Guard.Against.NillAndDefault(dateFrom).GetDayWithoutTime(),
            dateTo?.GetDayWithoutTime(),
            Guard.Against.NillAndDefault(duration),
            Guard.Against.NillAndDefault(createdBy)
        );
}

internal class HandleCreateWorkingHours : ICommandHandler<CreateWorkingHours>
{
    private readonly IRepository<WorkingHours> workingHoursRepository;
    private readonly WorkingHoursFactory factory;
    private readonly IDocumentSession querySession;

    public HandleCreateWorkingHours(IRepository<WorkingHours> workingHoursRepository, WorkingHoursFactory factory,
        IDocumentSession querySession)
    {
        this.workingHoursRepository = workingHoursRepository;
        this.factory = factory;
        this.querySession = querySession;
    }

    public async Task<Unit> Handle(CreateWorkingHours request, CancellationToken cancellationToken)
    {
        var periodOverlapExp = PeriodExpressions.GetPeriodOverlapExp<WorkingHours>(request);
        var workingHoursInThisPeriodExists = querySession.Query<WorkingHours>()
            .Any(periodOverlapExp.And(x => x.UserId == request.UserId));
        if (workingHoursInThisPeriodExists)
        {
            throw new ArgumentException("You cant add working hours in this period, because other overlap it");
        }
        var workingHours = factory.Create(request);
        await workingHoursRepository.Add(workingHours, cancellationToken);
        await workingHoursRepository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}
