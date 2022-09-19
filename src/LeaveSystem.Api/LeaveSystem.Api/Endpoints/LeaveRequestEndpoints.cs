﻿using GoldenEye.Commands;
using GoldenEye.Queries;
using LeaveSystem.Api.Responses;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveRequests.ApprovingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.CancellingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.RejectingLeaveRequest;
using Marten.Pagination;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints;
public static class LeaveRequestEndpoints
{
    public const string GetLeaveRequestsName = "GetLeaveRequests";
    public const string CreateLeaveRequestName = "CreateLeaveRequest";
    public const string AcceptLeaveRequestName = "AcceptLeaveRequest";
    public const string RejectLeaveRequestName = "RejectLeaveRequest";
    public const string CancelLeaveRequestName = "CancelLeaveRequest";
    public static void AddLeaveRequestEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/leaveRequests", async (HttpContext httpContext, IQueryBus queryBus, int? pageNumber, int? pageSize, DateTimeOffset? dateFrom, DateTimeOffset? dateTo) =>
        {
            var pagedList = await queryBus.Send<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>(GetLeaveRequests.Create(pageNumber, pageSize, dateFrom, dateTo));

            return PagedListResponse.From(pagedList);
        })
        .WithName(GetLeaveRequestsName)
        .RequireAuthorization(GetLeaveRequestsName);

        endpoint.MapPost("api/leaveRequests", async (HttpContext httpContext, ICommandBus commandBus, CreateLeaveRequestDto createLeaveRequest) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var leaveRequestId = Guid.NewGuid();
            var command = EventSourcing.LeaveRequests.CreatingLeaveRequest.CreateLeaveRequest.Create(
                leaveRequestId,
                createLeaveRequest.DateFrom,
                createLeaveRequest.DateTo,
                createLeaveRequest.Duration,
                createLeaveRequest.LeaveTypeId,
                createLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.Send(command);
            return Results.Created("api/LeaveRequests", leaveRequestId);
        })
        .WithName(CreateLeaveRequestName)
        .RequireAuthorization(CreateLeaveRequestName);

        endpoint.MapPost("api/leaveRequests/{id}/accept", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, AcceptLeaveRequestDto acceptLeaveRequest) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.AcceptingLeaveRequest.AcceptLeaveRequest.Create(
                id,
                acceptLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.Send(command);
            return Results.NoContent();
        })
        .WithName(AcceptLeaveRequestName)
        .RequireAuthorization(AcceptLeaveRequestName);

        endpoint.MapPost("api/leaveRequests/{id}/reject", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, RejectLeaveRequestDto rejectLeaveRequest) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.RejectingLeaveRequest.RejectLeaveRequest.Create(
                id,
                rejectLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.Send(command);
            return Results.NoContent();
        })
        .WithName(RejectLeaveRequestName)
        .RequireAuthorization(RejectLeaveRequestName);

        endpoint.MapPost("api/leaveRequests/{id}/cancel", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, CancelLeaveRequestDto cancelLeaveRequest) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.CancelingLeaveRequest.CancelLeaveRequest.Create(
                id,
                cancelLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.Send(command);
            return Results.NoContent();
        })
        .WithName(CancelLeaveRequestName)
        .RequireAuthorization(CancelLeaveRequestName);
    }
}
