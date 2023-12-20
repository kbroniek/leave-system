using LeaveSystem.Linq;
using LeaveSystem.Shared.LeaveRequests;
using Marten;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class UsedLeavesService
{
    private readonly IDocumentSession documentSession;

    public UsedLeavesService(IDocumentSession documentSession)
    {
        this.documentSession = documentSession;
    }
    public virtual async Task<TimeSpan> GetUsedLeavesDuration(
        DateTimeOffset firstDayOfYear,
        DateTimeOffset lastDayOfYear,
        string userId,
        Guid leaveTypeId,
        IEnumerable<Guid> nestedLeaveTypeIds)
    {
        var nestedLeaveTypeIdsExpression = PredicateBuilder
            .Create<LeaveRequestCreated>(x => x.LeaveTypeId == leaveTypeId);
        if (nestedLeaveTypeIds.Any())
        {
            nestedLeaveTypeIdsExpression = nestedLeaveTypeIdsExpression
                .Or(PredicateBuilder.MatchAny<LeaveRequestCreated, Guid>(x => x.LeaveTypeId,
                    nestedLeaveTypeIds.ToArray()));
        }
        var connectedLeaveRequestsCreatedExpression = PredicateBuilder.Create<LeaveRequestCreated>(x =>
                x.CreatedBy.Id == userId &&
                x.DateFrom >= firstDayOfYear &&
                x.DateTo <= lastDayOfYear)
            .And(nestedLeaveTypeIdsExpression);
        var leaveRequestCreatedEvents = await documentSession.Events.QueryRawEventDataOnly<LeaveRequestCreated>()
            .Where(connectedLeaveRequestsCreatedExpression).ToListAsync();

        TimeSpan usedLimits = TimeSpan.Zero;
        foreach (var @event in leaveRequestCreatedEvents)
        {
            var leaveRequestFromDb = await documentSession.Events.AggregateStreamAsync<LeaveRequest>(@event.StreamId);
            if (leaveRequestFromDb?.Status.IsValid() == true)
            {
                usedLimits += leaveRequestFromDb.Duration;
            }
        }

        return usedLimits;
    }
}
