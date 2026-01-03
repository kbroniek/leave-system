namespace LeaveSystem.Functions.Users;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Functions.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

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
}
