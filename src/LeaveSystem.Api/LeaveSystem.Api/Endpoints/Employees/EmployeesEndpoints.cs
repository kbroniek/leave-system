using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.Employees;

public static class EmployeesEndpoints
{
    public const string GetEmployeesPolicyName = "GetEmployees";
    public static IEndpointRouteBuilder AddEmployeesEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/employees", async (HttpContext httpContext, LeaveSystemDbContext dbContext, GetGraphUserService graphUserService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var graphUserTask = graphUserService.Get(cancellationToken);
            var userIdsTask = dbContext.Roles
                .Where(r => r.RoleType == RoleType.Employee)
                .Select(r => r.UserId)
                .ToListAsync(cancellationToken);
            await Task.WhenAll(graphUserTask, userIdsTask);
            var userIds = userIdsTask.Result;
            var graphUsers = graphUserTask.Result;
            return new GetEmployeesDto(userIds.Select(userId => Map(userId, graphUsers)));
        })
        .WithName(GetEmployeesPolicyName)
        .RequireAuthorization(GetEmployeesPolicyName);

        return endpoint;
    }

    private static GetEmployeeDto Map(string userId, IEnumerable<GraphUser> graphUsers)
    {
        var graphUser = graphUsers.FirstOrDefault(gu => string.Equals(gu.Id, userId, StringComparison.OrdinalIgnoreCase));
        return new GetEmployeeDto(userId, graphUser?.DisplayName ?? graphUser?.Email ?? $"{userId} unnamed", graphUser?.Email);
    }
}
