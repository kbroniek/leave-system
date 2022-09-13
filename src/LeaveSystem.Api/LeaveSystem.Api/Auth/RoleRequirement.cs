using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Api.Auth;

public class RoleRequirement : IAuthorizationRequirement
{
    public RoleRequirement(params RoleType[] roles) =>
        Roles = roles;

    public RoleType[] Roles { get; }
}

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly LeaveSystemDbContext dbContext;

    public RoleRequirementHandler(LeaveSystemDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            context.Fail(new AuthorizationFailureReason(this, $"The user is not authenticated."));
            return;
        }
        var userModel = context.User.CreateModel();
        var allRoles = requirement.Roles.Union(new RoleType[] { RoleType.GlobalAdmin }).ToArray();

        if (await dbContext.Roles.AnyAsync(r => r.Email == userModel.Email && allRoles.Contains(r.RoleName)))
        {
            context.Succeed(requirement);
            return;
        }
        context.Fail(new AuthorizationFailureReason(this, $"The user {userModel.Email} doesn't have access to the endpoint."));
    }
}