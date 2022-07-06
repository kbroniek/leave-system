using GoldenEye.Commands;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;

namespace LeaveSystem.Api.Endpoints;
public static class LeaveRequestEndpoints
{
    public static void AddLeaveRequestEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapPost("api/createLeaveRequest", (HttpContext httpContext, ICommandBus commandBus, Web.Pages.CreatingLeaveRequest.CreateLeaveRequestDto leaveRequest) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);
            var surname = httpContext.User.FindFirst(ClaimTypes.Surname);
            var givenName = httpContext.User.FindFirst(ClaimTypes.GivenName);
            var email1 = httpContext.User.FindFirst("emails");
            var command = EventSourcing.LeaveRequests.CreatingLeaveRequest.CreateLeaveRequest.Create(
                        Guid.NewGuid(),
                        leaveRequest.DateFrom,
                        leaveRequest.DateTo,
                        leaveRequest.Hours,
                        leaveRequest.Type,
                        leaveRequest.Remarks
                    );
            return commandBus.Send(command);

        })
        .WithName("CreateLeaveRequest")
        .RequireAuthorization();
    }
}
