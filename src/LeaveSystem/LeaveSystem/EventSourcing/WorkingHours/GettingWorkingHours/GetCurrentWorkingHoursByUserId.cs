using Ardalis.GuardClauses;
using GoldenEye.Backend.Core.DDD.Queries;
using LeaveSystem.Extensions;
using LeaveSystem.Shared;
using Marten;


namespace LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;

public class GetCurrentWorkingHoursByUserId : IQuery<WorkingHours>
{
    public string UserId { get; }
    private GetCurrentWorkingHoursByUserId(string userId) =>
        UserId = userId;

    public static GetCurrentWorkingHoursByUserId Create(string? userId) =>
        new(Guard.Against.NullOrWhiteSpace(userId));
}

internal class HandleGetWorkingHoursByUserId : IQueryHandler<GetCurrentWorkingHoursByUserId, WorkingHours>
{
    private readonly IDocumentSession querySession;

    public HandleGetWorkingHoursByUserId(IDocumentSession querySession) =>
        this.querySession = querySession;

    public async Task<WorkingHours> Handle(GetCurrentWorkingHoursByUserId request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now.GetDayWithoutTime();
        var workingHours = await querySession.GetCurrentWorkingHoursForUser(request.UserId, now, cancellationToken);
        return workingHours
               ?? throw GoldenEye.Backend.Core.Exceptions.NotFoundException.For<WorkingHours>(request.UserId);
    }
}
