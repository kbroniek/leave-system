using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis.Differencing;

namespace LeaveSystem.Web.Pages.WorkingHours;

public class WorkingHoursService
{
    private readonly HttpClient httpClient;

    public WorkingHoursService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public virtual async Task<PagedListResponse<WorkingHoursDto>?> GetWorkingHours(GetWorkingHoursQuery query)
    {
        var uri = query.CreateQueryString("api/workingHours");
        return await httpClient.GetFromJsonAsync<PagedListResponse<WorkingHoursDto>>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public virtual async Task<WorkingHoursDto?> GetUserWorkingHours(string userId)
    {
        var uri = $"api/workingHours/{userId}";
        try
        {
            return await httpClient.GetFromJsonAsync<WorkingHoursDto>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public virtual async Task Edit(WorkingHoursDto workingHoursDto)
    {
        var jsonString = JsonSerializer.Serialize(workingHoursDto);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync($"api/workingHours/{workingHoursDto.Id}/modify", httpContent);
        if (!response.IsSuccessStatusCode)
        {
            // TODO: Log an error
            throw new InvalidOperationException("Can't update working hours");
        }
    }
}