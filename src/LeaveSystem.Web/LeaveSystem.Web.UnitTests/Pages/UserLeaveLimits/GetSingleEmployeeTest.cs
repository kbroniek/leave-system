using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.UnitTests.TestStuff.Factories;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;

namespace LeaveSystem.Web.UnitTests.Pages.UserLeaveLimits;

public class GetSingleEmployeeTest
{
    private HttpClient httpClient;
    private EmployeeService GetSut() => new(httpClient);
    
    [Fact]
    public async Task WhenRequestReturnedResult_ThenReturnDeserializedEmployee()
    {
        //Given
        var employee = FakeGetEmployeesDtoProvider.GetAll().First();
        var url = $"api/employees/{employee.Id}";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, employee);
        var sut = GetSut();
        //When
        var result = await sut.Get(employee.Id);
        //Then
        result.Should().BeEquivalentTo(employee);
    }

    [Fact]
    public async Task WhenRequestReturnedNoResult_ThenReturnNull()
    {
        //Given
        GetEmployeeDto employee = null;
        var fakeId = Guid.NewGuid().ToString();
        var url = $"api/employees/{fakeId}";
        httpClient = HttpClientMockFactory.CreateWithJsonResponse(url, employee);
        var sut = GetSut();
        //When
        var result = await sut.Get(fakeId);
        //Then
        result.Should().BeNull();
    }
}