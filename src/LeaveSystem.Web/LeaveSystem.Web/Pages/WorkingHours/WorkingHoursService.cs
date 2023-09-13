using LeaveSystem.Shared.WorkingHours;
using System.Net.Http.Json;
using System.Text.Json;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;

namespace LeaveSystem.Web.Pages.WorkingHours;

public class WorkingHoursService
{
    private readonly HttpClient httpClient;

    public WorkingHoursService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public virtual async Task<PagedListResponse<EventSourcing.WorkingHours.WorkingHours>?> GetWorkingHours(GetWorkingHoursQuery query)
    {
        var uri = query.CreateQueryString("api/workingHours");
        return await httpClient.GetFromJsonAsync<PagedListResponse<EventSourcing.WorkingHours.WorkingHours>>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public virtual async Task<EventSourcing.WorkingHours.WorkingHours?> GetUserWorkingHours(string userId)
    {
        var uri = $"api/workingHours/{userId}";
        return await httpClient.GetFromJsonAsync<EventSourcing.WorkingHours.WorkingHours>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}

