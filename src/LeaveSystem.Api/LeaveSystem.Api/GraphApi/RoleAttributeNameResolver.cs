﻿using Ardalis.GuardClauses;
using LeaveSystem.Shared;

namespace LeaveSystem.Api.Factories;

public record class RoleAttributeNameResolver(string RoleAttributeName)
{
    private const string ShortRoleAttributeName = "Role";
    public static RoleAttributeNameResolver Create(string? b2cExtensionAppClientId)
    {
        Guard.Against.NullOrEmpty(b2cExtensionAppClientId);
        string roleAttributeName = GetCompleteAttributeName(b2cExtensionAppClientId, ShortRoleAttributeName);
        return new(roleAttributeName);
    }

    private static string GetCompleteAttributeName(string b2cExtensionAppClientId, string attributeName)
    {
        b2cExtensionAppClientId = b2cExtensionAppClientId.Replace("-", "");
        return $"extension_{b2cExtensionAppClientId}_{attributeName}";
    }

    public static RolesAttribute MapRoles(IDictionary<string, object>? additionalData, string roleAttributeName)
    {
        if (additionalData?.TryGetValue(roleAttributeName, out object? rolesRaw) == true && rolesRaw is not null)
        {
            return new RolesAttribute(FederatedUser.MapRoles(rolesRaw.ToString()));
        }
        return RolesAttribute.Empty;
    }
}
