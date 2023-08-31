using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using MediatR;

namespace LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;

public class CreateWorkingHours : ICommand
{
    public Guid WorkingHoursId { get; }
    public string UserId { get;}
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset DateTo { get; }
    public TimeSpan? Duration { get; }

    private CreateWorkingHours(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan? duration)
    {
        UserId = userId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        Duration = duration;
        WorkingHoursId = workingHoursId;
    }

    public static CreateWorkingHours Create(Guid? workingHoursId, string? userId, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, TimeSpan? duration) =>
        new(
            Guard.Against.NillAndDefault(workingHoursId),
            Guard.Against.NullOrWhiteSpace(userId),
            Guard.Against.NillAndDefault(dateFrom),
            Guard.Against.NillAndDefault(dateTo),
            duration
        );
}

internal class HandleCreateWorkingHours : ICommandHandler<CreateWorkingHours>
{
    private readonly IRepository<WorkingHours> repository;
    private readonly WorkingHoursFactory factory;

    public HandleCreateWorkingHours(IRepository<WorkingHours> repository, WorkingHoursFactory factory)
    {
        this.repository = repository;
        this.factory = factory;
    }

    public async Task<Unit> Handle(CreateWorkingHours request, CancellationToken cancellationToken)
    {
        var workingHours = factory.Create(request);
        await repository.Add(workingHours, cancellationToken);
        await repository.SaveChanges(cancellationToken);
        return Unit.Value;
    }
}