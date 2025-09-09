using System.Security.Claims;
using LeaveSystem.Shared.Auth;

namespace LeaveSystem.Shared;
public static class ClaimsPrincipalExtensions
{
    public static FederatedUser CreateModel(this ClaimsPrincipal claimsPrincipal)
    {
        var firstName = claimsPrincipal.FindFirst(ClaimTypes.Surname) ?? claimsPrincipal.FindFirst("family_name");
        var lastName = claimsPrincipal.FindFirst(ClaimTypes.GivenName) ?? claimsPrincipal.FindFirst("given_name");
        var id = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) ?? claimsPrincipal.FindFirst("oid") ?? claimsPrincipal.FindFirst("sub");
        var fullName = string.IsNullOrWhiteSpace(firstName?.Value) || string.IsNullOrWhiteSpace(lastName?.Value) ?
            claimsPrincipal.FindFirst("name")?.Value :
            $"{firstName.Value} {lastName.Value}";
        var email = claimsPrincipal.FindFirst("emails");
        // Roles are now fetched from CosmosDB instead of JWT claims
        // TODO: Fetch roles from CosmosDB
        return FederatedUser.Create(id?.Value, email?.Value, fullName, Enumerable.Empty<string>());
    }

    public static bool IsInRoles(this IEnumerable<string> roles, params RoleType[] roleTypes) =>
        roles.Any(r => roleTypes.Any(t => r.Equals(t.ToString(), StringComparison.Ordinal)));
}
