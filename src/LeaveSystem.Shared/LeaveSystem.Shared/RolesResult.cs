namespace LeaveSystem.Shared;

public record RolesResult(IEnumerable<string> Roles)
{
    public static RolesResult Empty => new(Enumerable.Empty<string>());
}