using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public class FakeGetEmployeesDtoProvider
{
    public static IEnumerable<GetEmployeeDto> GetAll()
    {
        return FakeUserProvider.GetEmployees().Select(GetEmployeeDto.Create);
    }
}