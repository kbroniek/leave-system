using Ardalis.GuardClauses;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.Employees;

public static class UsersEndpoints
{
    public const string GetUsersPolicyName = "GetUsers";
    public static IEndpointRouteBuilder AddUsersEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/users", async (HttpContext httpContext, GetGraphUserService graphUserService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var graphUsers = await graphUserService.Get(cancellationToken);
            return new UsersDto(graphUsers
                .Select(graphUser => Map(graphUser)));
        })
        .WithName(GetUsersPolicyName)
        .RequireAuthorization(GetUsersPolicyName);

        return endpoint;
    }

    private static UserDto Map(FederatedUser graphUser) =>
        new UserDto(graphUser.Id, graphUser.Name ?? graphUser.Email ?? $"{graphUser.Id} unnamed", graphUser.Email, graphUser.Roles);
}
