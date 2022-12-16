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
        endpoint.MapGet("api/employee", async (HttpContext httpContext, LeaveSystemDbContext dbContext, GraphUserService graphUserService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var graphUserTask = graphUserService.Get(cancellationToken);
            var emailsTask = dbContext.Roles
                .Where(r => r.RoleType == RoleType.Employee)
                .Select(r => r.Email)
                .ToListAsync(cancellationToken);
            await Task.WhenAll(graphUserTask, emailsTask);
            var emails = emailsTask.Result;
            var graphUsers = graphUserTask.Result;
            return new GetEmployeesDto(emails.Select(e => Map(e, graphUsers)));
        })
        .WithName(GetEmployeeEndpointsPolicyName)
        .RequireAuthorization(GetEmployeeEndpointsPolicyName);

        return endpoint;
    }

    private static GetEmployeeDto Map(string email, IEnumerable<GraphUser> graphUsers)
    {
        var graphUser = graphUsers.FirstOrDefault(gu => string.Equals(gu.Email, email, StringComparison.OrdinalIgnoreCase));
        return new GetEmployeeDto(graphUser?.DisplayName, email);
    }
}
