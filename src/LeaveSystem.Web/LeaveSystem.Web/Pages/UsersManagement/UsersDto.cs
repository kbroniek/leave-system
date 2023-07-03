using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

public record class UsersDto(IEnumerable<UserDto> Items);
public record class UserDto(string Id, string? Name, string? Email)
{
    public IEnumerable<string>? Roles { get; set; }
};
