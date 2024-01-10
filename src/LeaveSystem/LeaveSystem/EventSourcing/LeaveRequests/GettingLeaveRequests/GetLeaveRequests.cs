using Ardalis.GuardClauses;
using GoldenEye.Backend.Core.DDD.Queries;
using LeaveSystem.Linq;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.LeaveRequests;
using Marten;
using Marten.Pagination;

namespace LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;

public class GetLeaveRequests : IQuery<IPagedList<LeaveRequestShortInfo>>
{
    public static readonly LeaveRequestStatus[] ValidStatuses = { LeaveRequestStatus.Accepted, LeaveRequestStatus.Pending };
    public int PageNumber { get; }
    public int PageSize { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset DateTo { get; }
    public Guid[]? LeaveTypeIds { get; }
    public LeaveRequestStatus[] Statuses { get; }
    public string[]? CreatedByEmails { get; }
    public string[]? CreatedByUserIds { get; }
    public FederatedUser RequestedBy { get; }

    private GetLeaveRequests(int pageNumber, int pageSize, DateTimeOffset dateFrom, DateTimeOffset dateTo, Guid[]? leaveTypeIds, LeaveRequestStatus[] statuses, string[]? createdByEmails, string[]? createdByUserIds, FederatedUser requestedBy)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        DateFrom = dateFrom;
        DateTo = dateTo;
        LeaveTypeIds = leaveTypeIds;
        Statuses = statuses;
        CreatedByEmails = createdByEmails;
        CreatedByUserIds = createdByUserIds;
        RequestedBy = requestedBy;
    }

    public static GetLeaveRequests Create(int? pageNumber, int? pageSize, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, Guid[]? leaveTypeIds, LeaveRequestStatus[]? statuses, string[]? createdByEmails, string[]? createdByUserIds, FederatedUser requestedBy)
    {
        // TODO date provider
        var now = DateTimeOffset.UtcNow;
        const int defaultPageNumber = 1;
        var pageNumberOrDefault = pageNumber ?? defaultPageNumber;
        const int defaultPageSize = 20;
        var pageSizeOrDefault = pageSize ?? defaultPageSize;
        const int defaultDaysFrom = -14;
        var dateFromOrDefault = dateFrom ?? now.Add(TimeSpan.FromDays(defaultDaysFrom));
        const int defaultDaysTo = 14;
        var dateToOrDefault = dateTo ?? now.Add(TimeSpan.FromDays(defaultDaysTo));
        Guard.Against.NegativeOrZero(pageNumberOrDefault, nameof(pageNumber));
        const int minPageSize = 1;
        const int maxPageSize = 1000;
        Guard.Against.OutOfRange(pageSizeOrDefault, nameof(pageSize), minPageSize, maxPageSize);
        return new(
            pageNumberOrDefault,
            pageSizeOrDefault,
            dateFromOrDefault.GetDayWithoutTime(),
            dateToOrDefault.GetDayWithoutTime(),
            leaveTypeIds,
            statuses ?? ValidStatuses,
            createdByEmails,
            createdByUserIds,
            requestedBy);
    }
}


internal class HandleGetLeaveRequests :
    IQueryHandler<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>
{
    private readonly IDocumentSession querySession;

    public HandleGetLeaveRequests(IDocumentSession querySession) =>
        this.querySession = querySession;

    public async Task<IPagedList<LeaveRequestShortInfo>> Handle(GetLeaveRequests request,
        CancellationToken cancellationToken)
    {
        request = NarrowDownQuery(request);
        return await Query(request, cancellationToken);
    }

    private static GetLeaveRequests NarrowDownQuery(GetLeaveRequests request)
    {
        var privilegedRoles = new[]
        {
            RoleType.HumanResource.ToString(),
            RoleType.LeaveLimitAdmin.ToString(),
            RoleType.DecisionMaker.ToString(),
            RoleType.GlobalAdmin.ToString()
        };
        if (!request.RequestedBy.Roles.Any(r => privilegedRoles.Contains(r)))
        {
            return GetLeaveRequests.Create(
                request.PageNumber,
                request.PageSize,
                request.DateFrom,
                request.DateTo,
                request.LeaveTypeIds,
                request.Statuses,
                request.CreatedByEmails,
                new[] { request.RequestedBy.Id },
                request.RequestedBy);
        }
        return request;
    }

    private async Task<IPagedList<LeaveRequestShortInfo>> Query(GetLeaveRequests request, CancellationToken cancellationToken)
    {
        var query = querySession.Query<LeaveRequestShortInfo>()
                    .Where(x => (x.DateFrom >= request.DateFrom && x.DateFrom <= request.DateTo) ||
                                (x.DateTo >= request.DateFrom && x.DateTo <= request.DateTo) ||
                                (request.DateFrom >= x.DateFrom && request.DateFrom <= x.DateTo));

        if (request.Statuses.Length > 0)
        {
            var predicate = PredicateBuilder.False<LeaveRequestShortInfo>();
            foreach (var status in request.Statuses)
            {
                predicate = predicate.Or(x => x.Status == status);
            }
            query = query.Where(predicate);
        }
        if (request.LeaveTypeIds is { Length: > 0 })
        {
            var predicate = PredicateBuilder.False<LeaveRequestShortInfo>();
            foreach (var leaveTypeId in request.LeaveTypeIds)
            {
                predicate = predicate.Or(x => x.LeaveTypeId == leaveTypeId);
            }
            query = query.Where(predicate);
        }
        if (request.CreatedByEmails is { Length: > 0 })
        {
            var predicate = PredicateBuilder.False<LeaveRequestShortInfo>();
            foreach (var createdByEmail in request.CreatedByEmails)
            {
                predicate = predicate.Or(x => x.CreatedBy.Email == createdByEmail);
            }
            query = query.Where(predicate);
        }
        if (request.CreatedByUserIds is { Length: > 0 })
        {
            var predicate = PredicateBuilder.False<LeaveRequestShortInfo>();
            foreach (var createdByUserId in request.CreatedByUserIds)
            {
                predicate = predicate.Or(x => x.CreatedBy.Id == createdByUserId);
            }
            query = query.Where(predicate);
        }
        return await query
            .ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
