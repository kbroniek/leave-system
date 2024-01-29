using Ardalis.GuardClauses;
using LeaveSystem.Shared;

namespace LeaveSystem.GraphApi;

public record RoleAttributeNameResolver(string RoleAttributeName)
{
    private const string ShortRoleAttributeName = "Role";
    public static RoleAttributeNameResolver Create(string? b2cExtensionAppClientId)
    {
        Guard.Against.NullOrEmpty(b2cExtensionAppClientId);
        var roleAttributeName = GetCompleteAttributeName(b2cExtensionAppClientId, ShortRoleAttributeName);
        return new(roleAttributeName);
    }

    private static string GetCompleteAttributeName(string b2cExtensionAppClientId, string attributeName)
    {
        b2cExtensionAppClientId = b2cExtensionAppClientId.Replace("-", "");
        return $"extension_{b2cExtensionAppClientId}_{attributeName}";
    }

    public static RolesResult MapRoles(IDictionary<string, object?>? additionalData, string roleAttributeName)
    {
        if (additionalData?.TryGetValue(roleAttributeName, out var rolesRaw) == true && rolesRaw is not null)
        {
            return new RolesResult(FederatedUser.MapRoles(rolesRaw.ToString()));
        }
        return RolesResult.Empty;
    }
}

