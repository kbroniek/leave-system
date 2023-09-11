using Microsoft.AspNetCore.Authorization;

namespace LeaveSystem.Shared.Auth;

public class RoleRequirement : IAuthorizationRequirement
{
    public static RoleRequirement AuthorizeAll = new RoleRequirement(Enum.GetValues<RoleType>());
    public RoleRequirement(params RoleType[] roles) =>
        Roles = roles;

    public RoleType[] Roles { get; }
}

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail(new AuthorizationFailureReason(this, $"The user is not authenticated."));
            return Task.CompletedTask;
        }
        var userModel = context.User.CreateModel();
        var allRoles = requirement.Roles
            .Union(new RoleType[] { RoleType.GlobalAdmin })
            .Select(r => r.ToString())
            .ToArray();

        if (userModel.Roles.Any(r => allRoles.Contains(r)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        context.Fail(new AuthorizationFailureReason(this, $"The user {userModel.Email} doesn't have access to the endpoint. UserId: {userModel.Id}"));
        return Task.CompletedTask;
    }
}

public enum RoleType
{
    Employee,
    LeaveLimitAdmin,
    DecisionMaker,
    HumanResource,
    UserAdmin,
    GlobalAdmin
}
