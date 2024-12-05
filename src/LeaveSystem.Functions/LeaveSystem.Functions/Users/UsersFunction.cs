namespace LeaveSystem.Functions.Users;

using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using static LeaveSystem.Domain.LeaveRequests.Creating.IGetUserRepository;

public class UsersFunction(IGetUserRepository getUserRepository)
{

    [Function(nameof(GetUsers))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public async Task<IActionResult> GetUsers([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "users")] HttpRequest req,
        [CosmosDBInput(
            databaseName: "%DatabaseName%",
            containerName: "%RolesContainerName%",
            SqlQuery = "SELECT c.id, c.roles FROM c JOIN r IN c. roles",
            Connection  = "CosmosDBConnection")] IEnumerable<RolesDto> roles, CancellationToken cancellationToken)
    {
        var result = await getUserRepository.GetUsers([], cancellationToken);
        var rolesFreeze = roles.ToList();

        return result.Match(
            (employees) => new OkObjectResult(employees.Select(x => Map(x, rolesFreeze)).ToPagedListResponse()),
            error => error.ToObjectResult("Error occurred while getting roles."));
    }

    private static UserDto Map(User user, IReadOnlyCollection<RolesDto> roles) =>
        new(user.Id, user.Name, user.FirstName, user.LastName, roles.FirstOrDefault(x => x.Id == user.Id)?.Roles ?? []);

    public record RolesDto(string Id, string[] Roles);
}
