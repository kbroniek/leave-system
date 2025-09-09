using Ardalis.GuardClauses;
using System.Text.Json;

namespace LeaveSystem.Shared;

public record struct FederatedUser(string Id, string? Email, string? Name, IEnumerable<string> Roles)
{
    public static FederatedUser Create(string? federatedUserId, string? email = null, string? name = null) =>
        Create(federatedUserId, email, name, Enumerable.Empty<string>());
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
        // Roles are now fetched from CosmosDB instead of JWT claims
        // This method is kept for backward compatibility but will always return empty roles
        //TODO: Fetch roles from CosmosDB
        return RolesResult.Empty.Roles;
    }
}

