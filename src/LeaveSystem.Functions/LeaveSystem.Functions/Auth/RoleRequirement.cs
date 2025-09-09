using Microsoft.AspNetCore.Authorization;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared;

namespace LeaveSystem.Functions.Auth;

public class RoleRequirement(params RoleType[] roles) : IAuthorizationRequirement
{
    public static readonly RoleType[] AllRoles = Enum.GetValues<RoleType>();
    public static readonly RoleRequirement AuthorizeAll = new(AllRoles);

    public RoleType[] Roles { get; } = roles;
}

public class RoleRequirementHandler(IRolesRepository rolesRepository) : AuthorizationHandler<RoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Fail(new AuthorizationFailureReason(this, $"The user is not authenticated."));
            return;
        }

        var userModel = context.User.CreateModel();
        if (string.IsNullOrEmpty(userModel.Id))
        {
            context.Fail(new AuthorizationFailureReason(this, $"User ID is missing from claims."));
            return;
        }

        // Fetch roles from CosmosDB
        var rolesResult = await rolesRepository.GetUserRoles(userModel.Id, CancellationToken.None);
        if (rolesResult.IsFailure)
        {
            context.Fail(new AuthorizationFailureReason(this, $"Failed to retrieve user roles: {rolesResult.Error.Message}"));
            return;
        }

        var userRoles = rolesResult.Value.Roles;
        var allRoles = requirement.Roles
            .Union([RoleType.GlobalAdmin])
            .Select(r => r.ToString())
            .ToArray();

        if (userRoles.Any(r => allRoles.Contains(r)))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail(new AuthorizationFailureReason(this, $"The user {userModel.Email} doesn't have access to the endpoint. UserId: {userModel.Id}"));
    }
}
