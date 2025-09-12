namespace LeaveSystem.Functions.Users;

using System.Linq;
using LeaveSystem.Domain;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class RolesFunction(IRolesRepository rolesRepository)
{
    [Function(nameof(GetCurrentUserRoles))]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserRoles(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "roles/me")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var userId = req.HttpContext.GetUserId();
        var result = await rolesRepository.GetUserRoles(userId, cancellationToken);

        return result.Match<IActionResult>(
            userRoles => new OkObjectResult(new { roles = userRoles.Roles }),
            error => error.ToObjectResult("Error occurred while getting user roles."));
    }

    [Function(nameof(UpdateRole))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public RoleOutput UpdateRole(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "roles/{userId:guid}")] HttpRequest req,
        Guid userId, [FromBody] RoleDto role)
    {
        if (userId != role.Id)
        {
            return new()
            {
                Result = new Error($"{nameof(RoleDto.Id)} cannot be different than userId.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_ID_MISMATCH)
                    .ToObjectResult($"Error occurred while updating a role. userId = {role.Id}.")
            };
        }

        if (!role.Roles.All(x => Enum.GetNames<RoleType>().Contains(x)))
        {
            return new()
            {
                Result = new Error($"{nameof(RoleDto.Roles)} is out of range.", System.Net.HttpStatusCode.BadRequest, ErrorCodes.INVALID_ROLE)
                    .ToObjectResult($"Error occurred while updating a role. userId = {role.Id}.")
            };
        }

        return new()
        {
            Result = new OkObjectResult(role),
            Role = role
        };
    }

    public class RoleOutput
    {
        [HttpResult]
        public IActionResult Result { get; set; }

        [CosmosDBOutput("%DatabaseName%", "%RolesContainerName%",
            Connection = "CosmosDBConnection", CreateIfNotExists = true)]
        public RoleDto? Role { get; set; }
    }
}
