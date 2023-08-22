using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

public record GetEmployeesDto(IEnumerable<GetEmployeeDto> Items);
public record GetEmployeeDto(string Id, string? Name, string? Email)
{
    public static GetEmployeeDto Create(FederatedUser user) => new(user.Id, user.Name, user.Email);
}
