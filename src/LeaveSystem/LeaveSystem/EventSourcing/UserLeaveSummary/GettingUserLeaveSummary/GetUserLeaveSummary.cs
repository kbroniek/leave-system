using Ardalis.GuardClauses;
using GoldenEye.Queries;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.Shared;
using Marten.Pagination;

namespace LeaveSystem.EventSourcing.UserLeaveSummary.GettingUserLeaveSummary;

public class GetUserLeaveSummary : IQuery<UserLeaveSummaryInfo>
{
    public FederatedUser RequestedBy { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset DateTo { get; }
    private GetUserLeaveSummary(FederatedUser requestedBy, DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        RequestedBy = requestedBy;
        DateFrom = dateFrom;
        DateTo = dateTo;
    }

    public static GetUserLeaveSummary Create(FederatedUser requestedBy, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
    {
        var now = new Lazy<DateTimeOffset>(() => DateTimeOffset.Now);
        Guard.Against.Nill(requestedBy.Email);
        return new GetUserLeaveSummary(requestedBy, dateFrom ?? now.Value.GetFirstDayOfYear(), dateTo ?? now.Value.GetLastDayOfYear());
    }
}


internal class HandleGetUserLeaveSummary :
    IQueryHandler<GetUserLeaveSummary, UserLeaveSummaryInfo>
{
    private readonly IQueryBus queryBus;
    private readonly LeaveSystemDbContext dbContext;

    public HandleGetUserLeaveSummary(IQueryBus queryBus, LeaveSystemDbContext dbContext)
    {
        this.queryBus = queryBus;
        this.dbContext = dbContext;
    }

    public async Task<UserLeaveSummaryInfo> Handle(GetUserLeaveSummary request, CancellationToken cancellationToken)
    {
        var pagedList = await queryBus.Send<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>(GetLeaveRequests.Create(
            1,
            1000,
            request.DateFrom,
            request.DateTo,
            null,
            Enum.GetValues<LeaveRequestStatus>(),
            new[] { request.RequestedBy }
            ), cancellationToken);

    }
}
