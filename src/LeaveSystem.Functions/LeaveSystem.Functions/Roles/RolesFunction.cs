namespace LeaveSystem.Functions.Users;

using System.Linq;
using LeaveSystem.Domain;
using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class RolesFunction
{
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
                Result = new Error($"{nameof(RoleDto.Id)} cannot be different than userId.", System.Net.HttpStatusCode.BadRequest)
                    .ToObjectResult($"Error occurred while updating a role. userId = {role.Id}.")
            };
        }

        if (!role.Roles.All(x => Enum.GetNames<RoleType>().Contains(x)))
        {
            return new()
            {
                Result = new Error($"{nameof(RoleDto.Roles)} is out of range.", System.Net.HttpStatusCode.BadRequest)
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
