namespace LeaveSystem.Functions.Employees;

using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.IdentityModel.Tokens;

public class EmployeesFunction(IGetUserRepository getUserRepository)
{

    [Function("GetEmployee")]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.DecisionMaker)},{nameof(RoleType.HumanResource)},{nameof(RoleType.LeaveLimitAdmin)}")]
    public async Task<IActionResult> GetEmployees([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "employees")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%RolesContainerName%",
            SqlQuery = $"SELECT c.id FROM c JOIN r IN c. roles WHERE r = \"{nameof(RoleType.Employee)}\"",
            Connection  = "CosmosDBConnection")] IEnumerable<RolesDto> roles, CancellationToken cancellationToken)
    {
        var result = !roles.IsNullOrEmpty() ? await getUserRepository.GetUsers(roles.Select(x => x.Id).ToArray(), cancellationToken) : Result.Ok<IReadOnlyCollection<IGetUserRepository.User>, Error>([]);

        return result.Match(
            (employees) => new OkObjectResult(employees.ToPagedListResponse()),
            error => error.ToObjectResult("Error occurred while getting roles."));
    }
    public record RolesDto(string Id);
}

