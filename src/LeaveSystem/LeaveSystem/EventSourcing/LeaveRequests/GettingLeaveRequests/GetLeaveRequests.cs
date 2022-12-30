﻿using Ardalis.GuardClauses;
using GoldenEye.Queries;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Linq;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using Marten;
using Marten.Pagination;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;

public class GetLeaveRequests : IQuery<IPagedList<LeaveRequestShortInfo>>
{
    public readonly static LeaveRequestStatus[] validStatuses = new[] { LeaveRequestStatus.Accepted, LeaveRequestStatus.Pending };
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
        var now = DateTimeOffset.UtcNow;
        var pageNumberOrDefault = pageNumber ?? 1;
        var pageSizeOrDefault = pageSize ?? 20;
        var dateFromOrDefault = dateFrom ?? now.Add(TimeSpan.FromDays(-14));
        var dateToOrDefault = dateTo ?? now.Add(TimeSpan.FromDays(14));
        Guard.Against.NegativeOrZero(pageNumberOrDefault, nameof(pageNumber));
        Guard.Against.OutOfRange(pageSizeOrDefault, nameof(pageSize), 1, 1000);
        return new(
            pageNumberOrDefault,
            pageSizeOrDefault,
            dateFromOrDefault.GetDayWithoutTime(),
            dateToOrDefault.GetDayWithoutTime(),
            leaveTypeIds,
            statuses ?? validStatuses,
            createdByEmails,
            createdByUserIds,
            requestedBy);
    }
}


internal class HandleGetLeaveRequests :
    IQueryHandler<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>
{
    private readonly IDocumentSession querySession;
    private readonly LeaveSystemDbContext dbContext;

    public HandleGetLeaveRequests(IDocumentSession querySession, LeaveSystemDbContext dbContext)
    {
        this.querySession = querySession;
        this.dbContext = dbContext;
    }

    public async Task<IPagedList<LeaveRequestShortInfo>> Handle(GetLeaveRequests request,
        CancellationToken cancellationToken)
    {
        request = await NarrowDownQuery(request, cancellationToken);
        return await Query(request, cancellationToken);
    }

    private async Task<GetLeaveRequests> NarrowDownQuery(GetLeaveRequests request,
        CancellationToken cancellationToken)
    {
        var privilegedRoles = new RoleType[] { RoleType.HumanResource, RoleType.LeaveLimitAdmin, RoleType.DecisionMaker, RoleType.GlobalAdmin };
        if (!await EntityFrameworkQueryableExtensions.AnyAsync(dbContext.Roles, r => r.UserId == request.RequestedBy.Id && privilegedRoles.Contains(r.RoleType), cancellationToken))
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
        if (request.LeaveTypeIds != null && request.LeaveTypeIds.Length > 0)
        {
            var predicate = PredicateBuilder.False<LeaveRequestShortInfo>();
            foreach (var leaveTypeId in request.LeaveTypeIds)
            {
                predicate = predicate.Or(x => x.LeaveTypeId == leaveTypeId);
            }
            query = query.Where(predicate);
        }
        if (request.CreatedByEmails != null && request.CreatedByEmails.Length > 0)
        {
            var predicate = PredicateBuilder.False<LeaveRequestShortInfo>();
            foreach (var createdByEmail in request.CreatedByEmails)
            {
                predicate = predicate.Or(x => x.CreatedBy.Email == createdByEmail);
            }
            query = query.Where(predicate);
        }
        if (request.CreatedByUserIds != null && request.CreatedByUserIds.Length > 0)
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
