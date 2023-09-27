using System.Net;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared.Extensions;

namespace LeaveSystem.Web.Pages.WorkingHours;

public class WorkingHoursService
{
    private readonly HttpClient httpClient;
    private readonly IToastService toastService;

    public WorkingHoursService(HttpClient httpClient, IToastService toastService)
    {
        this.httpClient = httpClient;
        this.toastService = toastService;
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

    public virtual async Task<bool> Edit(IEnumerable<WorkingHoursDto> workingHoursDtos)
    {
        foreach (var workingHoursDto in workingHoursDtos)
        {
            var jsonString = JsonSerializer.Serialize(workingHoursDto);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"api/workingHours/{workingHoursDto.Id}/modify", httpContent);
            if (response.IsSuccessStatusCode) continue;
            // TODO: Log an error
            var responseMessage = await response.Content.ReadAsStringAsync();
            toastService.ShowError(responseMessage);
            return false;
        }
        toastService.ShowSuccess("Working hours updated successfully");
        return true;
    }
    
    public virtual async Task<List<WorkingHoursDto>?> AddAndReturnDtos(IEnumerable<AddWorkingHoursDto> addWorkingHoursDtos)
    {
        var resultWorkingHours = new List<WorkingHoursDto>();
        foreach (var addWorkingHoursDto in addWorkingHoursDtos)
        {
            var jsonString = JsonSerializer.Serialize(addWorkingHoursDto);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"api/workingHours", httpContent);
            if (response.IsSuccessStatusCode)
            {
                var workingHoursId = await response.Content.ReadFromJsonAsync<Guid>();
                resultWorkingHours.Add(addWorkingHoursDto.ToWorkingHoursDto(workingHoursId));
                continue;
            }
            // TODO: Log an error
            var responseMessage = await response.Content.ReadFromJsonAsync<string>() ?? string.Empty;
            toastService.ShowError(responseMessage);
            return null;
        }
        toastService.ShowSuccess("Working hours added successfully");
        return resultWorkingHours;
    }
}