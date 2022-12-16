using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.Employees;

public static class EmployeesEndpoints
{
    public const string GetEmployeeEndpointsPolicyName = "GetEmployee";
    public static IEndpointRouteBuilder AddEmployeesEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/employee", async (HttpContext httpContext, LeaveSystemDbContext dbContext, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var emails = await dbContext.Roles
                .Where(r => r.RoleType == RoleType.Employee)
                .Select(r => r.Email)
                .ToListAsync(cancellationToken);
            return new GetEmployeesDto(emails.Select(e => new GetEmployeeDto("", e)));
        })
        .WithName(GetEmployeeEndpointsPolicyName)
        .RequireAuthorization(GetEmployeeEndpointsPolicyName);

        return endpoint;
    }
}
