namespace LeaveSystem.Functions.Users;

using LeaveSystem.Functions.Extensions;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class UsersFunction
{
    private readonly ILogger<UsersFunction> logger;

    public UsersFunction(ILogger<UsersFunction> logger)
    {
        this.logger = logger;
    }

    [Function(nameof(GetUsers))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public IActionResult GetUsers([HttpTrigger(
        AuthorizationLevel.Anonymous,
        "get",
        Route = "users")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var userId = req.HttpContext.GetUserId();
        var users = new[]
        {
            new UserDto(userId, req.HttpContext.User.Identity?.Name, "test@email.com", new [] { nameof(RoleType.GlobalAdmin) })
        };
        return new OkObjectResult(users.ToPagedListResponse());
    }
}
