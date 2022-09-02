using GoldenEye.Commands;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints;
public static class LeaveRequestEndpoints
{
    public static void AddLeaveRequestEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapPost("api/createLeaveRequest", (HttpContext httpContext, ICommandBus commandBus, Web.Pages.CreatingLeaveRequest.CreateLeaveRequestDto createLeaveRequest) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.CreatingLeaveRequest.CreateLeaveRequest.Create(
                Guid.NewGuid(),
                createLeaveRequest.DateFrom,
                createLeaveRequest.DateTo,
                createLeaveRequest.Duration,
                createLeaveRequest.LeaveTypeId,
                createLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            return commandBus.Send(command);

        })
        .WithName("CreateLeaveRequest")
        .RequireAuthorization();

        endpoint.MapPost("api/approveLeaveRequest", (HttpContext httpContext, ICommandBus commandBus, Web.Pages.ApprovingLeaveRequest.ApproveLeaveRequestDto approveLeaveRequest) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.ApprovingLeaveRequest.ApproveLeaveRequest.Create(
                approveLeaveRequest.LeaveRequestId,
                approveLeaveRequest.Remarks,
                httpContext.User.CreateModel()
            );
            return commandBus.Send(command);
        })
        .WithName("ApproveLeaveRequest")
        .RequireAuthorization();
    }
}
