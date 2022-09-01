namespace LeaveSystem.Db;

public class FederatedUser
{
    public string? Name { get; set; }
    public string? Email { get; set; }

    public static FederatedUser Create(string? email, string? name)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentNullException(nameof(email));
        }
        return new()
        {
            Email = email,
            Name = name,
        };
    }
}

