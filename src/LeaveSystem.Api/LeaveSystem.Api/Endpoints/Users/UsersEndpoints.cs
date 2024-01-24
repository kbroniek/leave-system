using Ardalis.GuardClauses;
using LeaveSystem.Domain.Employees;
using LeaveSystem.Domain.Users;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.UsersManagement;
using Microsoft.Identity.Web.Resource;

namespace LeaveSystem.Api.Endpoints.Users;

public static class UsersEndpoints
{
    public const string GetUsersPolicyName = "GetUsers";
    public const string AddUserPolicyName = "AddUser";
    public const string UpdateUserPolicyName = "UpdateUser";
    public static IEndpointRouteBuilder AddUsersEndpoints(this IEndpointRouteBuilder endpoint, string azureScpes)
    {
        endpoint.MapGet("api/users", async (HttpContext httpContext, GetGraphUserService graphUserService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            var graphUsers = await graphUserService.Get(cancellationToken);
            return new UsersDto(graphUsers
                .Select(graphUser => ToDto(graphUser)));
        })
        .WithName(GetUsersPolicyName)
        .RequireAuthorization(GetUsersPolicyName);

        endpoint.MapPost("api/users", async (UserDto userToAdd, HttpContext httpContext, SaveGraphUserService graphUserService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            Guard.Against.InvalidEmail(userToAdd.Email);

            var addedUser = await graphUserService.Add(userToAdd.Email, userToAdd.Name, userToAdd.Roles ?? Enumerable.Empty<string>(), cancellationToken);

            return Results.Created("api/users", addedUser.Id);
        })
        .WithName(AddUserPolicyName)
        .RequireAuthorization(AddUserPolicyName);

        endpoint.MapPut("api/users/{userId}", async (string? userId, UserDto userToUpdate, HttpContext httpContext, SaveGraphUserService graphUserService, CancellationToken cancellationToken) =>
        {
            httpContext.VerifyUserHasAnyAcceptedScope(azureScpes);

            Guard.Against.Null(userId);
            Guard.Against.InvalidEmail(userToUpdate.Email);

            await graphUserService.Update(userId, userToUpdate.Email, userToUpdate.Name, userToUpdate.Roles ?? Enumerable.Empty<string>(), cancellationToken);

            return Results.NoContent();
        })
        .WithName(UpdateUserPolicyName)
        .RequireAuthorization(UpdateUserPolicyName);

        return endpoint;
    }

    private static UserDto ToDto(FederatedUser graphUser) =>
        new UserDto(graphUser.Id, graphUser.Name ?? graphUser.Email ?? $"{graphUser.Id} unnamed", graphUser.Email, graphUser.Roles);
}
