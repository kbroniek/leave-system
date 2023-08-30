namespace LeaveSystem.Web.Pages.UsersManagement;

public record UsersDto(IEnumerable<UserDto> Items);
public class UserDto
{
    public UserDto() { }
    public UserDto(string? id, string? name, string? email, IEnumerable<string>? roles)
    {
        Id = id;
        Name = name;
        Email = email;
        Roles = roles;
    }

    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public static UserDto Create() =>
        new();
}