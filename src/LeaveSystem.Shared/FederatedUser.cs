using Ardalis.GuardClauses;

namespace LeaveSystem.Shared;

public record struct FederatedUser(string Email, string? Name)
{
    public static FederatedUser Create(string? email, string? name)
    {
        Guard.Against.Nill(email);
        return new(email, name);
    }
}

