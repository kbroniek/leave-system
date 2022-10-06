using System.Text.Json.Serialization;

namespace LeaveSystem.Web;

public struct FederatedUser
{
    public string Email { get; }
    public string? Name { get; }

    [JsonConstructor]
    public FederatedUser(string email, string? name) =>
        (Email, Name) = (email, name);
}
