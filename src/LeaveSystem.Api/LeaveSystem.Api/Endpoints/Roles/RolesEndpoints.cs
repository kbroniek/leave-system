using LeaveSystem.Web.Pages.Roles;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.Roles;

public static class RolesEndpoints
{
    public const string GetRolesPolicyName = "GetRoles";
    public static IEndpointRouteBuilder AddRolesEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/roles", async (HttpContext httpContext, UserRolesGraphService userRolesGraphService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var graphUsersRoles = await userRolesGraphService.Get(cancellationToken);
            return new GetRolesDto(graphUsersRoles.Select(ur => new GetUserRolesDto(ur.Id, ur.Roles)));
        })
        .WithName(GetRolesPolicyName)
        .RequireAuthorization(GetRolesPolicyName);

        return endpoint;
    }
}
