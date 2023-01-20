using Ardalis.GuardClauses;
using System.Text.Json;

namespace LeaveSystem.Shared;

public record struct FederatedUser(string Id, string? Email, string? Name, IEnumerable<string> Roles)
{
    public static FederatedUser Create(string? federatedUserId, string? email, string? name)
    {
        return Create(federatedUserId, email, name, Enumerable.Empty<string>());
    }
    public static FederatedUser Create(string? federatedUserId, string? email, string? name, IEnumerable<string> roles)
    {
        Guard.Against.NullOrWhiteSpace(federatedUserId);
        return new(federatedUserId, email, name, roles);
    }
    public static FederatedUser Create(string? federatedUserId, string? email, string? name, string? rolesRaw)
    {
        Guard.Against.NullOrWhiteSpace(federatedUserId);
        return new(federatedUserId, email, name, MapRoles(rolesRaw));
    }

    public static IEnumerable<string> MapRoles(string? rolesRaw)
    {
        if (string.IsNullOrWhiteSpace(rolesRaw))
        {
            return RolesAttribute.Empty.Roles;
        }
        var roleAttribute = JsonSerializer.Deserialize<RolesAttribute?>(rolesRaw) ?? RolesAttribute.Empty;
        return roleAttribute.Roles;
    }
}

