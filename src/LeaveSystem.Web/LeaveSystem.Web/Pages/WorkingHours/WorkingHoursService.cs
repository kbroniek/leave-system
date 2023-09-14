using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using System.Net.Http.Json;
using System.Text.Json;

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
        return await httpClient.GetFromJsonAsync<WorkingHoursDto>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}

public record WorkingHoursDto(string UserId, DateTimeOffset DateFrom, DateTimeOffset DateTo, TimeSpan? Duration, WorkingHoursStatus Status);

