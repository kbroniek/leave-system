using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace LeaveSystem.Shared.Auth;

public class RoleRequirement : IAuthorizationRequirement
{
    public static readonly RoleType[] AllRoles = Enum.GetValues<RoleType>();
    public static readonly RoleRequirement AuthorizeAll = new(AllRoles);
    public RoleRequirement(params RoleType[] roles) =>
        Roles = roles;

    public RoleType[] Roles { get; }
}

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly ILogger<RoleRequirementHandler> logger;

    public RoleRequirementHandler(ILogger<RoleRequirementHandler> logger)
    {
        this.logger = logger;
    }
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail(new AuthorizationFailureReason(this, $"The user is not authenticated."));
            return Task.CompletedTask;
        }
        var allRoles = requirement.Roles
            .Union(new RoleType[] { RoleType.GlobalAdmin })
            .Select(r => r.ToString());

        if (allRoles.Any(context.User.IsInRole))
        {
            logger.LogWarning("Correct user");
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        logger.LogWarning($"unauthorized user {context.User.FindFirst(ClaimTypes.Role)?.Value}");
        logger.LogWarning(string.Join(",", context.User.Claims.Select(c => $"{c.Type} - {c.Value}")));
        logger.LogWarning($"The user {context.User.Identity.Name} doesn't have access to the endpoint. UserId: {context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
        context.Fail(new AuthorizationFailureReason(this, $"The user {context.User.Identity.Name} doesn't have access to the endpoint. UserId: {context.User.FindFirst("sub")?.Value}"));
        return Task.CompletedTask;
    }
}

public enum RoleType
{
    Employee,
    LeaveLimitAdmin,
    LeaveTypeAdmin,
    DecisionMaker,
    HumanResource,
    UserAdmin,
    GlobalAdmin,
    WorkingHoursAdmin
}

