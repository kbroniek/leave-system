using Ardalis.GuardClauses;
using GoldenEye.Backend.Core.DDD.Queries;
using LeaveSystem.Domain.Employees.GettingEmployees;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.Employees;

public static class EmployeesEndpoints
{
    public const string GetEmployeesPolicyName = "GetEmployees";
    public const string GetEmployeePolicyName = "GetEmployee";
    public static IEndpointRouteBuilder AddEmployeesEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/employees", async (HttpContext httpContext, IQueryBus queryBus, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var federatedUser = httpContext.User.CreateModel();
            var graphUsers = await queryBus.SendAsync<GetEmployees, IEnumerable<FederatedUser>>(new GetEmployees(federatedUser), cancellationToken);
            return new GetEmployeesDto(graphUsers.Select(graphUser => Map(graphUser)));
        })
        .WithName(GetEmployeesPolicyName)
        .RequireAuthorization(GetEmployeesPolicyName);

        endpoint.MapGet("api/employees/{id}", async (HttpContext httpContext, string? id, IQueryBus queryBus, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            Guard.Against.NullOrEmpty(id);

            var federatedUser = httpContext.User.CreateModel();
            var graphUser = await queryBus.SendAsync<GetSingleEmployee, FederatedUser>(new GetSingleEmployee(federatedUser, id), cancellationToken);
            return Map(graphUser);
        })
        .WithName(GetEmployeePolicyName)
        .RequireAuthorization(GetEmployeePolicyName);

        return endpoint;
    }

    private static GetEmployeeDto Map(FederatedUser graphUser) =>
        new(graphUser.Id, graphUser.Name ?? graphUser.Email ?? $"{graphUser.Id} unnamed", graphUser.Email);
}
