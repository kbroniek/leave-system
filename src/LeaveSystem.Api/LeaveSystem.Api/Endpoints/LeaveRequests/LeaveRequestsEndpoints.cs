using GoldenEye.Backend.Core.DDD.Commands;
using GoldenEye.Backend.Core.DDD.Queries;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequestDetails;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.CancellingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.RejectingLeaveRequest;
using Marten.Pagination;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.LeaveRequests;

public static class LeaveRequestsEndpoints
{
    public const string GetLeaveRequestsPolicyName = "GetLeaveRequests";
    public const string GetLeaveRequestDetailsPolicyName = "GetLeaveRequestDetails";
    public const string CreateLeaveRequestPolicyName = "CreateLeaveRequest";
    public const string CreateLeaveRequestOnBehalfPolicyName = "CreateLeaveRequestOnBehalf";
    public const string AcceptLeaveRequestPolicyName = "AcceptLeaveRequest";
    public const string RejectLeaveRequestPolicyName = "RejectLeaveRequest";
    public const string CancelLeaveRequestPolicyName = "CancelLeaveRequest";
    public static IEndpointRouteBuilder AddLeaveRequestEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/leaveRequests", async (HttpContext httpContext, IQueryBus queryBus, GetLeaveRequestsQuery query, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var pagedList = await queryBus.SendAsync<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>(GetLeaveRequests.Create(
                query.PageNumber,
                query.PageSize,
                query.DateFrom,
                query.DateTo,
                query.LeaveTypeIds,
                query.Statuses,
                query.CreatedByEmails,
                query.CreatedByUserIds,
                httpContext.User.CreateModel()), cancellationToken);

            return PagedListResponse.From(pagedList);
        })
        .WithName(GetLeaveRequestsPolicyName)
        .RequireAuthorization(GetLeaveRequestsPolicyName)
        .Produces(StatusCodes.Status200OK);

        endpoint.MapGet("api/leaveRequests/{id}", (HttpContext httpContext, IQueryBus queryBus, Guid? id, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);
            //TODO: Protect, only authorized users have access to all leave requests.
            return queryBus.SendAsync<GetLeaveRequestDetails, LeaveRequest>(GetLeaveRequestDetails.Create(id), cancellationToken);
        })
        .WithName(GetLeaveRequestDetailsPolicyName)
        .RequireAuthorization(GetLeaveRequestDetailsPolicyName);

        endpoint.MapPost("api/leaveRequests", async (HttpContext httpContext, ICommandBus commandBus, CreateLeaveRequestDto createLeaveRequest, CancellationToken cancellationToken) =>
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
            await commandBus.SendAsync(command, cancellationToken);
            return Results.Created("api/LeaveRequests", leaveRequestId);
        })
        .WithName(CreateLeaveRequestPolicyName)
        .RequireAuthorization(CreateLeaveRequestPolicyName)
        .Produces(StatusCodes.Status201Created);


        endpoint.MapPost("api/leaveRequests/onBehalf", async (HttpContext httpContext, ICommandBus commandBus, CreateLeaveRequestOnBehalfDto createLeaveRequest, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var leaveRequestId = Guid.NewGuid();
            var command = EventSourcing.LeaveRequests.CreatingLeaveRequest.CreateLeaveRequestOnBehalf.Create(
                leaveRequestId,
                createLeaveRequest.DateFrom,
                createLeaveRequest.DateTo,
                createLeaveRequest.Duration,
                createLeaveRequest.LeaveTypeId,
                createLeaveRequest.Remarks,
                createLeaveRequest.CreatedByOnBehalf,
                httpContext.User.CreateModel()
            );
            await commandBus.SendAsync(command, cancellationToken);
            return Results.Created("api/LeaveRequests", leaveRequestId);
        })
        .WithName(CreateLeaveRequestOnBehalfPolicyName)
        .RequireAuthorization(CreateLeaveRequestOnBehalfPolicyName);

        endpoint.MapPut("api/leaveRequests/{id}/accept", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, AcceptLeaveRequestDto acceptLeaveRequest, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.AcceptingLeaveRequest.AcceptLeaveRequest.Create(
                id,
                acceptLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.SendAsync(command, cancellationToken);
            return Results.NoContent();
        })
        .WithName(AcceptLeaveRequestPolicyName)
        .RequireAuthorization(AcceptLeaveRequestPolicyName)
        .Produces(StatusCodes.Status204NoContent);

        endpoint.MapPut("api/leaveRequests/{id}/reject", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, RejectLeaveRequestDto rejectLeaveRequest, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.RejectingLeaveRequest.RejectLeaveRequest.Create(
                id,
                rejectLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.SendAsync(command, cancellationToken);
            return Results.NoContent();
        })
        .WithName(RejectLeaveRequestPolicyName)
        .RequireAuthorization(RejectLeaveRequestPolicyName)
        .Produces(StatusCodes.Status204NoContent);

        endpoint.MapPut("api/leaveRequests/{id}/cancel", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, CancelLeaveRequestDto cancelLeaveRequest, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.CancelingLeaveRequest.CancelLeaveRequest.Create(
                id,
                cancelLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            await commandBus.SendAsync(command, cancellationToken);
            return Results.NoContent();
        })
        .WithName(CancelLeaveRequestPolicyName)
        .RequireAuthorization(CancelLeaveRequestPolicyName)
        .Produces(StatusCodes.Status204NoContent);

        return endpoint;
    }
}
