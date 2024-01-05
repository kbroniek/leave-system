using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LeaveSystem.Web.Pages.UsersManagement;

public class UsersService
{
    private readonly HttpClient httpClient;

    public UsersService(HttpClient httpClient) => this.httpClient = httpClient;

    public async Task<IEnumerable<UserDto>> Get()
    {
        var usersFromApi = await httpClient.GetFromJsonAsync<UsersDto>("api/users", new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return usersFromApi?.Items.Where(e => e != null) ?? Enumerable.Empty<UserDto>();
    }
    public async Task Edit(UserDto user)
    {
        var jsonString = JsonSerializer.Serialize(user);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync($"api/users/{user.Id}", httpContent);
        if (!response.IsSuccessStatusCode)
        {
            // TODO: Log an error
            throw new InvalidOperationException("Can't update users");
        }
    }
    public async Task<string> Create(UserDto user)
    {
        var jsonString = JsonSerializer.Serialize(user);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("api/users", httpContent);
        if (!response.IsSuccessStatusCode)
        {
            // TODO: Log an error
            throw new InvalidOperationException("Can't update users");
        }
        return await response.Content.ReadAsStringAsync();
    }
    public async Task Delete(string userId)
    {
        var response = await httpClient.DeleteAsync($"api/users/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            // TODO: Log an error
            throw new InvalidOperationException("Can't update users");
        }
    }
}
