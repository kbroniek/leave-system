using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using Marten;
using MediatR;

namespace LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;

public class AddWorkingHours : ICommand
{
    public Guid WorkingHoursId { get; }
    public string UserId { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset? DateTo { get; }
    public TimeSpan Duration { get; }

    private AddWorkingHours(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo,
        TimeSpan duration)
    {
        UserId = userId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        WorkingHoursId = workingHoursId;
    }

    public static AddWorkingHours Create(Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo, TimeSpan? duration) =>
        new(
            Guard.Against.NillAndDefault(workingHoursId),
            Guard.Against.NullOrWhiteSpace(userId),
            Guard.Against.NillAndDefault(dateFrom),
            dateTo,
            Guard.Against.NillAndDefault(duration)
        );
}

internal class HandleAddWorkingHours : ICommandHandler<AddWorkingHours>
{
    private readonly IRepository<WorkingHours> repository;
    private readonly WorkingHoursFactory factory;
    private readonly IDocumentSession querySession;

    public HandleAddWorkingHours(IRepository<WorkingHours> repository, WorkingHoursFactory factory,
        IDocumentSession querySession)
    {
        this.repository = repository;
        this.factory = factory;
        this.querySession = querySession;
    }

    public async Task<Unit> Handle(AddWorkingHours request, CancellationToken cancellationToken)
    {
        var currentWorkingHoursForUser = await querySession.Query<WorkingHours>()
            .Where(x => x.UserId == request.UserId && x.Status == WorkingHoursStatus.Current)
            .FirstOrDefaultAsync(cancellationToken);
        if (currentWorkingHoursForUser is not null)
        {
            currentWorkingHoursForUser.Deprecate();
            await repository.Update(currentWorkingHoursForUser, cancellationToken);
            await repository.SaveChanges(cancellationToken);
        }
        var createdWorkingHours = factory.Create(request);
        await repository.Add(createdWorkingHours, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}