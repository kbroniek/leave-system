using Ardalis.GuardClauses;

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
}

