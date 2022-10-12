using GoldenEye.Queries;
using LeaveSystem.Db;
using Marten;

namespace LeaveSystem.EventSourcing.UserLeaveSummary.GettingUserLeaveSummary;

public class GetUserLeaveSummary : IQuery<UserLeaveSummaryInfo>
{
    public FederatedUser RequestedBy { get; }
    public int Year { get; }
    private GetUserLeaveSummary(FederatedUser requestedBy, int year)
    {
        RequestedBy = requestedBy;
        Year = year;
    }

    public static GetUserLeaveSummary Create(FederatedUser requestedBy, int? year)
    {
        return new GetUserLeaveSummary(requestedBy, year ?? DateTimeOffset.Now.Year);
    }
}


internal class HandleGetUserLeaveSummary :
    IQueryHandler<GetUserLeaveSummary, UserLeaveSummaryInfo>
{
    private readonly IDocumentSession querySession;
    private readonly LeaveSystemDbContext dbContext;

    public HandleGetUserLeaveSummary(IDocumentSession querySession, LeaveSystemDbContext dbContext)
    {
        this.querySession = querySession;
        this.dbContext = dbContext;
    }

    public Task<UserLeaveSummaryInfo> Handle(GetUserLeaveSummary request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
