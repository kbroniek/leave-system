using Ardalis.GuardClauses;
using GoldenEye.Queries;
using LeaveSystem.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.WorkingHours;
using Marten;
using Marten.Pagination;

namespace LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;

public class GetWorkingHours : IQuery<IPagedList<WorkingHours>>
{
    public static readonly WorkingHoursStatus[] ValidStatuses = { WorkingHoursStatus.Current };
    public int PageNumber { get; }
    public int PageSize { get; }
    public DateTimeOffset DateFrom { get; }
    public DateTimeOffset DateTo { get; }
    public string[]? UserIds { get; }
    public FederatedUser RequestedBy { get; }
    public WorkingHoursStatus[] Statuses { get; }

    private GetWorkingHours(int pageSize, int pageNumber, DateTimeOffset dateFrom, DateTimeOffset dateTo,
        string[]? userIds, FederatedUser requestedBy, WorkingHoursStatus[] statuses)
    {
        DateFrom = dateFrom;
        PageSize = pageSize;
        PageNumber = pageNumber;
        DateTo = dateTo;
        UserIds = userIds;
        RequestedBy = requestedBy;
        Statuses = statuses;
    }

    public static GetWorkingHours Create(int? pageSize, int? pageNumber, DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo,
        string[]? userIds, FederatedUser requestedBy, WorkingHoursStatus[]? statuses)
    {
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
            pageSizeOrDefault, pageNumberOrDefault, dateFromOrDefault.GetDayWithoutTime(),
            dateToOrDefault.GetDayWithoutTime(), userIds, requestedBy, statuses ?? ValidStatuses
        );
    }
}

internal class HandleGetWorkingHours : IQueryHandler<GetWorkingHours, IPagedList<WorkingHours>>
{
    private readonly IDocumentSession querySession;

    public HandleGetWorkingHours(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public Task<IPagedList<WorkingHours>> Handle(GetWorkingHours request, CancellationToken cancellationToken)
    {
        request = NarrowDownQuery(request);
        return Query(request, cancellationToken);
    }

    private GetWorkingHours NarrowDownQuery(GetWorkingHours request)
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
            return GetWorkingHours.Create(
                request.PageNumber,
                request.PageSize,
                request.DateFrom,
                request.DateTo,
                new[] { request.RequestedBy.Id },
                request.RequestedBy,
                request.Statuses);
        }

        return request;
    }

    private async Task<IPagedList<WorkingHours>> Query(GetWorkingHours request, CancellationToken cancellationToken)
    {
        var query = querySession.Query<WorkingHours>()
            .Where(x => (x.DateFrom >= request.DateFrom && x.DateFrom <= request.DateTo) ||
                        (x.DateTo >= request.DateFrom && x.DateTo <= request.DateTo) ||
                        (request.DateFrom >= x.DateFrom && request.DateFrom <= x.DateTo));
        query = query.WhereMatchAny(w => w.UserId, request.UserIds);
        query = query.WhereMatchAny(w => w.Status, request.Statuses);
        return await query.ToPagedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}