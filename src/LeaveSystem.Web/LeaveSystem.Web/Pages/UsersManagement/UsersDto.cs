using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

public record class UsersDto(IEnumerable<UserDto> Items);
public class UserDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public static UserDto Create() =>
        new UserDto();
};
