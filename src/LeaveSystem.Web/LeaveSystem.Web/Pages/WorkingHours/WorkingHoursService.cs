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
        return await httpClient.GetFromJsonAsync<PagedListResponse<WorkingHoursDto>>(uri,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public virtual async Task<WorkingHoursDto?> GetUserWorkingHours(string userId)
    {
        var uri = $"api/workingHours/{userId}";
        try
        {
            return await httpClient.GetFromJsonAsync<WorkingHoursDto>(uri,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public virtual async Task<bool> Edit(WorkingHoursDto workingHoursDto)
    {
        var jsonString = JsonSerializer.Serialize(workingHoursDto);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync($"api/workingHours/{workingHoursDto.Id}/modify", httpContent);
        if (response.IsSuccessStatusCode)
        {
            toastService.ShowSuccess("Working hours updated successfully");
            return true;
        }

        // TODO: Log an error
        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>();
        var message = problemDto?.Title ?? "Something went wrong";
        toastService.ShowError(message);
        return false;
    }

    public virtual async Task<WorkingHoursDto?> AddAndReturnDto(AddWorkingHoursDto addWorkingHoursDto)
    {
        var jsonString = JsonSerializer.Serialize(addWorkingHoursDto);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"api/workingHours", httpContent);
        if (response.IsSuccessStatusCode)
        {
            var resultWorkingHoursDto = await response.Content.ReadFromJsonAsync<WorkingHoursDto>();
            if (resultWorkingHoursDto is not null)
            {
                toastService.ShowSuccess("Working hours added successfully");
            }
            else
            {
                toastService.ShowError("Unexpected empty result");
            }
            return resultWorkingHoursDto;
        }
        // TODO: Log an error
        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>();
        var message = problemDto?.Title ?? "Something went wrong";
        toastService.ShowError(message);
        return null;
    }
}