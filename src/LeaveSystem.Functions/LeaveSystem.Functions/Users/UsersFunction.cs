namespace LeaveSystem.Functions.Users;

using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Functions.GraphApi;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using static LeaveSystem.Domain.LeaveRequests.Creating.IGetUserRepository;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

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

    [Function(nameof(UpdateUser))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public async Task<IActionResult> UpdateUser(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "patch",
            Route = "users/{userId}")] HttpRequest req,
        string userId,
        [FromBody] UpdateUserDto updateUserDto,
        CancellationToken cancellationToken)
    {
        if (updateUserDto is null)
        {
            return new Error("Update user data is required.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT)
                .ToObjectResult($"Error occurred while updating user. UserId={userId}.");
        }

        if (!updateUserDto.AccountEnabled.HasValue && updateUserDto.JobTitle is null)
        {
            return new Error("At least one property (accountEnabled or jobTitle) must be provided.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT)
                .ToObjectResult($"Error occurred while updating user. UserId={userId}.");
        }

        var result = await updateUserRepository.UpdateUser(userId, updateUserDto, cancellationToken);

        return result.Match<IActionResult>(
            () => new OkObjectResult(new { message = "User updated successfully", userId }),
            error => error.ToObjectResult($"Error occurred while updating user. UserId={userId}."));
    }

    private static UserDto Map(User user, IReadOnlyCollection<RolesDto> roles) =>
        new(user.Id, user.Name, user.FirstName, user.LastName, roles.FirstOrDefault(x => x.Id == user.Id)?.Roles ?? [], user.JobTitle);

    public record RolesDto(string Id, string[] Roles);
}
