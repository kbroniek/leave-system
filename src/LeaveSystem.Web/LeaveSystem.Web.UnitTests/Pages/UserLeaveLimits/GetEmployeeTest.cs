using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;

namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

public class GetEmployeeTest
{
    private HttpClient httpClient;

    private EmployeeService GetSut() => new(httpClient);

    [Fact]
    public async Task WhenRequestReturnedResults_ThenDeserializeAndReturnOnlyNotNull()
    {
        //Given
        var employees = FakeGetEmployeesDtoProvider.GetAll();
        var employeesDtoWithNullItems = new GetEmployeesDto(employees.Append(null));
        var url = "api/employees";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, employeesDtoWithNullItems);
        var sut = GetSut();
        //When
        var result = await sut.Get();
        //Then
        result.Should().BeEquivalentTo(employees);
    }
    
    [Fact]
    public async Task WhenRequestReturnedNull_ThenReturnEmptyCollection()
    {
        //Given
        GetEmployeesDto? employees = null;
        var url = "api/employees";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, employees);
        var sut = GetSut();
        //When
        var result = await sut.Get();
        //Then
        result.Should().BeEmpty();
    }
}