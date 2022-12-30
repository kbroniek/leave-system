using LeaveSystem.Shared;

namespace LeaveSystem.Web.Extensions;

public static class FederatedUserExtensions
{
    public static string? GetEmail(this FederatedUser user)
    {
        return user.Email?.Substring(2, user.Email.Length - 4); // because we get the email in weird format i.e. ['example@test.com']
    }
}
