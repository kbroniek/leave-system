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
    public IActionResult GetUsers([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var userId = req.HttpContext.GetUserId();
        var users = new[]
        {
            new UserDto(userId, req.HttpContext.User.Identity?.Name, "test@email.com", new [] { nameof(RoleType.GlobalAdmin) })
        };
        return new OkObjectResult(users.ToPagedListResponse());
    }

    [Function(nameof(EditUser))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public IActionResult EditUser([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "users/{userId:alpha}")] HttpRequest req, string userId, [FromBody] EditUserDto user)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult(new UserDto(userId, user.Name, user.Email, user.Roles));
    }

    [Function(nameof(CreateUser))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public IActionResult CreateUser([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequest req, [FromBody] CreateUserDto user)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var userId = Guid.NewGuid().ToString();
        return new CreatedResult($"/users/{userId}", new UserDto(userId, user.Name, user.Email, user.Roles));
    }

    [Function(nameof(DisableUser))]
    [Authorize(Roles = $"{nameof(RoleType.GlobalAdmin)},{nameof(RoleType.UserAdmin)}")]
    public IActionResult DisableUser([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "users/{userId:alpha}")] HttpRequest req, string userId)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        return new NoContentResult();
    }
}
