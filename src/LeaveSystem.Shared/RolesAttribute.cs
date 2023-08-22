namespace LeaveSystem.Shared;

public record struct RolesAttribute(IEnumerable<string> Roles)
{
    public static RolesAttribute Empty => new(Enumerable.Empty<string>());
}