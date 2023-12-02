using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Shared;

public abstract class UniversalHttpService<T> where T : class
{
    private readonly HttpClient httpClient;
    private readonly IToastService toastService;
    private readonly ILogger logger;
    private readonly string entityName;

    protected UniversalHttpService(HttpClient httpClient, IToastService toastService, ILogger logger, string entityName)
    {
        this.httpClient = httpClient;
        this.toastService = toastService;
        this.logger = logger;
        this.entityName = entityName;
    }

    public virtual async Task<T?> AddAsync(string uri, T entityToAdd)
    {
        var jsonString = JsonSerializer.Serialize(entityToAdd);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(uri, httpContent);
        if (response.IsSuccessStatusCode)
        {
            var resultWorkingHoursDto = await response.Content.ReadFromJsonAsync<T>();
            if (resultWorkingHoursDto is not null)
            {
                toastService.ShowSuccess($"{entityName} added successfully");
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

    public virtual async Task<bool> EditAsync(string uri, T entityToEdit)
    {
        var jsonString = JsonSerializer.Serialize(entityToEdit);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync(uri, httpContent);
        if (response.IsSuccessStatusCode)
        {
            toastService.ShowSuccess($"{entityName} updated successfully");
            return true;
        }
        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>();
        logger.LogError("{Message}", problemDto?.Detail);
        var message = problemDto?.Title ?? "Something went wrong";
        toastService.ShowError(message);
        return false;
    }
    
    public virtual async Task<T?> GetSingleAsync(string uri)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (HttpRequestException ex)
        {
            toastService.ShowError($"Error occured while getting {entityName}");
            logger.LogError("{Message}", ex.Message);
            return null;
        }
    }

    public virtual async Task<IEnumerable<T>?> GetManyAsync(string uri)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<IEnumerable<T>>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (HttpRequestException ex)
        {
            toastService.ShowError("Error occured while getting working hours");
            logger.LogError("{Message}", ex.Message);
            return null;
        }
    }
}