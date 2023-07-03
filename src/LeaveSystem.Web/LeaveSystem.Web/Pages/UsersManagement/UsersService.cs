using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using System.Net.Http.Json;
using System.Text.Json;

namespace LeaveSystem.Web.Pages.UsersManagement;

public class UsersService
{
    private readonly HttpClient httpClient;

    public UsersService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IEnumerable<UserDto>> Get()
    {
        var usersFromApi = await httpClient.GetFromJsonAsync<UsersDto>("api/users", new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return usersFromApi?.Items.Where(e => e != null) ?? Enumerable.Empty<UserDto>();
    }
}
