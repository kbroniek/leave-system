namespace LeaveSystem.Functions.Users;

using System.Linq;
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
    public async Task<UpdateUserOutput> UpdateUser(
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
            return new UpdateUserOutput
            {
                Result = new Error("Update user data is required.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT)
                    .ToObjectResult($"Error occurred while updating user. UserId={userId}.")
            };
        }

        var hasUserProperties = updateUserDto.AccountEnabled.HasValue || updateUserDto.JobTitle is not null;
        var hasRoles = updateUserDto.Roles is not null;

        if (!hasUserProperties && !hasRoles)
        {
            return new UpdateUserOutput
            {
                Result = new Error("At least one property (accountEnabled, jobTitle, or roles) must be provided.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT)
                    .ToObjectResult($"Error occurred while updating user. UserId={userId}.")
            };
        }

        // Validate roles if provided
        if (hasRoles && !updateUserDto.Roles!.All(x => Enum.GetNames<RoleType>().Contains(x)))
        {
            return new UpdateUserOutput
            {
                Result = new Error($"{nameof(UpdateUserDto.Roles)} contains invalid role values.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_ROLE)
                    .ToObjectResult($"Error occurred while updating user. UserId={userId}.")
            };
        }

        // Update user properties in Graph API if provided
        if (hasUserProperties)
        {
            var userUpdateDto = new UpdateUserDto(updateUserDto.AccountEnabled, updateUserDto.JobTitle, null);
            var userResult = await updateUserRepository.UpdateUser(userId, userUpdateDto, cancellationToken);

            if (userResult.IsFailure)
            {
                return new UpdateUserOutput
                {
                    Result = userResult.Error.ToObjectResult($"Error occurred while updating user. UserId={userId}.")
                };
            }
        }

        // Prepare role output if roles are provided
        RoleDto? roleDto = null;
        if (hasRoles)
        {
            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                return new UpdateUserOutput
                {
                    Result = new Error("Invalid userId format. Must be a valid GUID for role updates.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT)
                        .ToObjectResult($"Error occurred while updating user. UserId={userId}.")
                };
            }
            roleDto = new RoleDto(userIdGuid, updateUserDto.Roles!);
        }

        return new UpdateUserOutput
        {
            Result = new OkObjectResult(new { message = "User updated successfully", userId }),
            Role = roleDto
        };
    }

    public class UpdateUserOutput
    {
        [HttpResult]
        public IActionResult Result { get; set; } = null!;

        [CosmosDBOutput("%DatabaseName%", "%RolesContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)]
        public RoleDto? Role { get; set; }
    }

    private static UserDto Map(User user, IReadOnlyCollection<RolesDto> roles) =>
        new(user.Id, user.Name, user.FirstName, user.LastName, roles.FirstOrDefault(x => x.Id == user.Id)?.Roles ?? [], user.JobTitle);

    public record RolesDto(string Id, string[] Roles);
}
