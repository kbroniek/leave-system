using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using System.Net.Http.Json;
using System.Text.Json;

namespace LeaveSystem.Web.Pages.UserLeaveLimits;

public class EmployeeService
{
    private readonly HttpClient httpClient;

    public EmployeeService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public Task<GetEmployeeDto?> Get(string id) =>
        httpClient.GetFromJsonAsync<GetEmployeeDto>($"api/employees/{id}", new JsonSerializerOptions(JsonSerializerDefaults.Web));
    public async Task<IEnumerable<GetEmployeeDto>> Get()
    {
        var employeesFromApi = await httpClient.GetFromJsonAsync<GetEmployeesDto>("api/employees", new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return employeesFromApi?.Items.Where(e => e != null) ?? Enumerable.Empty<GetEmployeeDto>();
    }

}

