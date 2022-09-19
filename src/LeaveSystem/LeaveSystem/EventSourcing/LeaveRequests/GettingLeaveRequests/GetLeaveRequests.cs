using Ardalis.GuardClauses;
using GoldenEye.Queries;
using LeaveSystem.Shared;
using Marten;
using Marten.Pagination;

namespace LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;

public class GetLeaveRequests : IQuery<IPagedList<LeaveRequestShortInfo>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset DateTo { get; }

    private GetLeaveRequests(int pageNumber, int pageSize, DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        DateFrom = dateFrom;
        DateTo = dateTo;
    }

    public static GetLeaveRequests Create(int? pageNumber, int? pageSize, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
    {
        var now = DateTimeOffset.UtcNow;
        var pageNumberOrDefault = pageNumber ?? 1;
        var pageSizeOrDefault = pageSize ?? 20;
        var dateFromOrDefault = dateFrom ?? now.Add(TimeSpan.FromDays(-7));
        var dateToOrDefault = dateTo ?? now.Add(TimeSpan.FromDays(7));
        Guard.Against.NegativeOrZero(pageNumberOrDefault, nameof(pageNumber));
        Guard.Against.OutOfRange(pageSizeOrDefault, nameof(pageSize), 1, 100);
        return new(pageNumberOrDefault, pageSizeOrDefault, dateFromOrDefault.GetDayWithoutTime(), dateToOrDefault.GetDayWithoutTime());
    }
}

internal class HandleGetLeaveRequest :
    IQueryHandler<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>
{
    private readonly IDocumentSession querySession;

    public HandleGetLeaveRequest(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public async Task<IPagedList<LeaveRequestShortInfo>> Handle(GetLeaveRequests request,
        CancellationToken cancellationToken)
    {
        return await querySession.Query<LeaveRequestShortInfo>()
            .Where(x => ((x.DateFrom >= request.DateFrom && x.DateFrom <= request.DateTo) ||
                (x.DateTo >= request.DateFrom && x.DateTo <= request.DateTo) ||
                (request.DateFrom >= x.DateFrom && request.DateFrom <= x.DateTo)) &&
                (x.Status == LeaveRequestStatus.Accepted || x.Status == LeaveRequestStatus.Pending))
            .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
