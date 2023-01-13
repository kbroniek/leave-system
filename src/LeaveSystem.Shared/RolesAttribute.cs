namespace LeaveSystem.Shared;

public record RolesAttribute(IEnumerable<string> Roles)
{
    public static RolesAttribute Empty => new RolesAttribute(Enumerable.Empty<string>());
}