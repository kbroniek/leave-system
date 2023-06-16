using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

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

}

