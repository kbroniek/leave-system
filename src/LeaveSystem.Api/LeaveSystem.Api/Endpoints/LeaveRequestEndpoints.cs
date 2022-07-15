using GoldenEye.Commands;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints;
public static class LeaveRequestEndpoints
{
    public static void AddLeaveRequestEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapPost("api/createLeaveRequest", (HttpContext httpContext, ICommandBus commandBus, Web.Pages.CreatingLeaveRequest.CreateLeaveRequestDto leaveRequest) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var command = EventSourcing.LeaveRequests.CreatingLeaveRequest.CreateLeaveRequest.Create(
                        Guid.NewGuid(),
                        leaveRequest.DateFrom,
                        leaveRequest.DateTo,
                        leaveRequest.Hours == null ? null : TimeSpan.FromHours(leaveRequest.Hours.Value),
                        leaveRequest.LeaveTypeId,
                        leaveRequest.Remarks,
                        httpContext.User.CreateModel()
                    );
            return commandBus.Send(command);

        })
        .WithName("CreateLeaveRequest")
        .RequireAuthorization();
    }
}
