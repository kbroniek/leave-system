using Ardalis.GuardClauses;
using LeaveSystem.Shared;

namespace LeaveSystem.Db;

public record struct FederatedUser(string Email, string? Name)
{
    public static FederatedUser Create(string? email, string? name)
    {
        Guard.Against.Nill(email);
        return new(email, name);
    }
}

