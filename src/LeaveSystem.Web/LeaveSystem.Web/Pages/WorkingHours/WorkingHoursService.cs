using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;

namespace LeaveSystem.Web.Pages.WorkingHours;

public class WorkingHoursService
{
    private readonly HttpClient httpClient;
    private readonly IToastService toastService;
    private readonly ILogger<WorkingHoursService> logger;

    public WorkingHoursService(HttpClient httpClient, IToastService toastService, ILogger<WorkingHoursService> logger)
    {
        this.httpClient = httpClient;
        this.toastService = toastService;
        this.logger = logger;
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
        catch (HttpRequestException ex)
        {
            toastService.ShowError("Error occured while getting working hours");
            logger.LogError("{Message}", ex.Message);
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
        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>();
        logger.LogError("{Message}", problemDto?.Detail);
        var message = problemDto?.Title ?? "Something went wrong";
        toastService.ShowError(message);
        return false;
    }

    public virtual async Task<WorkingHoursDto?> Add(AddWorkingHoursDto addWorkingHoursDto)
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
        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>();
        logger.LogError("{Message}", problemDto?.Detail);
        var message = problemDto?.Title ?? "Something went wrong";
        toastService.ShowError(message);
        return null;
    }
}