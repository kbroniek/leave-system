using Ardalis.GuardClauses;

namespace LeaveSystem.Shared;

public record struct FederatedUser(string Id, string? Email, string? Name)
{
    public static FederatedUser Create(string? federatedUserId, string? email, string? name)
    {
        Guard.Against.NullOrWhiteSpace(federatedUserId);
        return new(federatedUserId, email, name);
    }
}

