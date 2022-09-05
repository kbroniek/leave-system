using GoldenEye.Commands;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints;
public static class LeaveRequestEndpoints
{
    public static void AddLeaveRequestEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapPost("api/leaveRequests", async (HttpContext httpContext, ICommandBus commandBus, Web.Pages.CreatingLeaveRequest.CreateLeaveRequestDto createLeaveRequest) =>
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
        .WithName("CreateLeaveRequest")
        .RequireAuthorization();

        endpoint.MapPost("api/leaveRequests/{id}/accept", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, Web.Pages.AcceptingLeaveRequest.AcceptLeaveRequestDto acceptLeaveRequest) =>
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
        .WithName("AcceptLeaveRequest")
        .RequireAuthorization();

        endpoint.MapPost("api/leaveRequests/{id}/reject", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, Web.Pages.RejectingLeaveRequest.RejectLeaveRequestDto rejectLeaveRequest) =>
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
        .WithName("RejectLeaveRequest")
        .RequireAuthorization();

        endpoint.MapPost("api/leaveRequests/{id}/cancel", async (HttpContext httpContext, ICommandBus commandBus, Guid? id, Web.Pages.CancellingLeaveRequest.CancelLeaveRequestDto cancelLeaveRequest) =>
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
        .WithName("CancelLeaveRequest")
        .RequireAuthorization();
    }
}
