using System.Security.Claims;
using System.Text.Json;

namespace LeaveSystem.Shared;
public static class ClaimsPrincipalExtensions
{
    public static FederatedUser CreateModel(this ClaimsPrincipal claimsPrincipal)
    {
        var firstName = claimsPrincipal.FindFirst(ClaimTypes.Surname) ?? claimsPrincipal.FindFirst("family_name");
        var lastName = claimsPrincipal.FindFirst(ClaimTypes.GivenName) ?? claimsPrincipal.FindFirst("given_name");
        var id = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier) ?? claimsPrincipal.FindFirst("oid");
        var fullName = string.IsNullOrWhiteSpace(firstName?.Value) || string.IsNullOrWhiteSpace(lastName?.Value) ?
            claimsPrincipal.FindFirst("name")?.Value :
            $"{firstName.Value} {lastName.Value}";
        var email = claimsPrincipal.FindFirst("emails");
        var rolesClaim = claimsPrincipal.FindFirst("extension_Role");
        var roles = Create(rolesClaim?.Value);
        return FederatedUser.Create(id?.Value, email?.Value, fullName, roles);
    }

    public static IEnumerable<string> Create(string? rolesRaw)
    {
        if (string.IsNullOrWhiteSpace(rolesRaw))
        {
            return RolesAttribute.Empty.Roles;
        }
        var roleAttribute = JsonSerializer.Deserialize<RolesAttribute>(rolesRaw) ?? RolesAttribute.Empty;
        return roleAttribute.Roles;
    }
}
