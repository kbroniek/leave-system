using Ardalis.GuardClauses;
using GoldenEye.Commands;
using GoldenEye.Queries;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using Marten;
using Marten.Linq;

namespace LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;

public class GetWorkingHoursByUserId : IQuery<WorkingHours>
{
    public string UserId { get; }
    private GetWorkingHoursByUserId(string userId)
    {
        UserId = userId;
    }

    public static GetWorkingHoursByUserId Create(string? userId) =>
        new(Guard.Against.NullOrWhiteSpace(userId));
}

internal class HandleGetWorkingHoursByUserId : IQueryHandler<GetWorkingHoursByUserId, WorkingHours>
{
    private readonly IDocumentSession querySession;

    public HandleGetWorkingHoursByUserId(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public async Task<WorkingHours> Handle(GetWorkingHoursByUserId request, CancellationToken cancellationToken)
    {
        var workingHours = await querySession.Query<WorkingHours>().Where(wh => wh.UserId == request.UserId && wh.Status == WorkingHoursStatus.Current)
            .FirstOrDefaultAsync(cancellationToken);
        return workingHours
               ?? throw GoldenEye.Exceptions.NotFoundException.For<WorkingHours>(request.UserId);
    }
}