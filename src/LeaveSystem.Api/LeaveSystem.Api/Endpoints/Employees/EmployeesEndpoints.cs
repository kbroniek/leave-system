using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.Employees;

public static class EmployeesEndpoints
{
    public const string GetEmployeesPolicyName = "GetEmployees";
    public static IEndpointRouteBuilder AddEmployeesEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/employees", async (HttpContext httpContext, GetGraphUserService graphUserService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var graphUsers = await graphUserService.Get(cancellationToken);
            return new GetEmployeesDto(graphUsers
                .Where(graphUser => graphUser.Roles.Any(r => r == RoleType.Employee.ToString()))
                .Select(graphUser => Map(graphUser)));
        })
        .WithName(GetEmployeesPolicyName)
        .RequireAuthorization(GetEmployeesPolicyName);

        return endpoint;
    }

    private static GetEmployeeDto Map(FederatedUser graphUser)
    {
        return new GetEmployeeDto(graphUser.Id, graphUser.Name ?? graphUser.Email ?? $"{graphUser.Id} unnamed", graphUser.Email);
    }
}
