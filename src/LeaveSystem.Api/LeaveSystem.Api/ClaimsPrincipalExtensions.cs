using LeaveSystem.Db;
using System.Security.Claims;

namespace LeaveSystem.Api;
public static class ClaimsPrincipalExtensions
{
    public static FederatedUser CreateModel(this ClaimsPrincipal claimsPrincipal)
    {
        var firstName = claimsPrincipal.FindFirst(ClaimTypes.Surname);
        var lastName = claimsPrincipal.FindFirst(ClaimTypes.GivenName);
        var fullName = string.IsNullOrWhiteSpace(firstName?.Value) || string.IsNullOrWhiteSpace(lastName?.Value) ?
            claimsPrincipal.FindFirst("name")?.Value :
            $"{firstName.Value} {lastName.Value}";
        var email = claimsPrincipal.FindFirst("emails");
        return FederatedUser.Create(email?.Value, fullName);
    }
}
