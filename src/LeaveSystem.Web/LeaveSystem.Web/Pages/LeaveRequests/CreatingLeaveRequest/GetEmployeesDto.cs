using LeaveSystem.Shared;

namespace LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

public record class GetEmployeesDto(IEnumerable<GetEmployeeDto> Items);
public record class GetEmployeeDto(string Id, string? Name, string? Email)
{
    public static GetEmployeeDto Create(FederatedUser user) =>
        new GetEmployeeDto(user.Id, user.Name, user.Email);
}
