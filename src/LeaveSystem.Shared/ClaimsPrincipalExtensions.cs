using System.Security.Claims;

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
        return FederatedUser.Create(id?.Value, email?.Value, fullName);
    }
}
