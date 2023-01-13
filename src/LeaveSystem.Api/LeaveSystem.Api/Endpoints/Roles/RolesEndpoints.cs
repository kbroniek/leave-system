using LeaveSystem.Web.Pages.Roles;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.Roles;

public static class RolesEndpoints
{
    public const string GetRolesPolicyName = "GetRoles";
    public const string UpdateRolesPolicyName = "UpdateRoles";
    public static IEndpointRouteBuilder AddEmployeesEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/roles", async (HttpContext httpContext, UserRolesGraphService userRolesGraphService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var graphUsersRoles = await userRolesGraphService.Get(cancellationToken);
            return new GetRolesDto(graphUsersRoles.Select(ur => new GetUserRolesDto(ur.Id, ur.Roles)));
        })
        .WithName(GetRolesPolicyName)
        .RequireAuthorization(GetRolesPolicyName);

        endpoint.MapPost("api/roles/{id}", async (HttpContext httpContext, string id, UpdateUserRoleDto userRoles, UserRolesGraphService graphUserService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            await graphUserService.Update(id, userRoles.Roles, cancellationToken);
            return Results.NoContent();
        })
        .WithName(UpdateRolesPolicyName)
        .RequireAuthorization(UpdateRolesPolicyName);

        return endpoint;
    }
}
