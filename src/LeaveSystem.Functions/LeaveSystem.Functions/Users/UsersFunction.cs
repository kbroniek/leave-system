namespace LeaveSystem.Functions.Users;

using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.Users;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Functions.GraphApi;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using static LeaveSystem.Domain.LeaveRequests.Creating.IGetUserRepository;

public class UsersFunction(IGetUserRepository getUserRepository, UpdateUserRepository updateUserRepository)
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
            error => error.ToObjectResult("Error occurred while getting users."));
    }

    [Function(nameof(DisableUser))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public async Task<IActionResult> DisableUser(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "patch",
            Route = "users/{userId}/disable")] HttpRequest req,
        string userId,
        CancellationToken cancellationToken)
    {
        var result = await updateUserRepository.EnableUser(userId, false, cancellationToken);

        return result.Match<IActionResult>(
            () => new OkObjectResult(new { message = "User disabled successfully", userId }),
            error => error.ToObjectResult($"Error occurred while disabling user. UserId={userId}."));
    }

    private static UserDto Map(User user, IReadOnlyCollection<RolesDto> roles) =>
        new(user.Id, user.Name, user.FirstName, user.LastName, roles.FirstOrDefault(x => x.Id == user.Id)?.Roles ?? [], user.JobTitle);

    public record RolesDto(string Id, string[] Roles);
}
