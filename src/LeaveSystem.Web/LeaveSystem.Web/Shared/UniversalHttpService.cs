using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.Web.Shared;

public abstract class UniversalHttpService
{
    private readonly HttpClient httpClient;
    private readonly IToastService toastService;
    private readonly ILogger logger;
    //Todo: dodać jako klasę wstrzykiwaną, a nie taką po której się dziedziczy
    protected UniversalHttpService(HttpClient httpClient, IToastService toastService, ILogger logger)
    {
        this.httpClient = httpClient;
        this.toastService = toastService;
        this.logger = logger;
    }

    protected virtual async Task<TResponse?> AddAsync<TContent, TResponse>(string uri, TContent entityToAdd,
        string successMessage, JsonSerializerOptions options)
        where TContent : class
        where TResponse : class
    {
        var jsonString = JsonSerializer.Serialize(entityToAdd, options);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(uri, httpContent);
        if (response.IsSuccessStatusCode)
        {
            var resultWorkingHoursDto = await response.Content.ReadFromJsonAsync<TResponse>();
            if (resultWorkingHoursDto is not null)
            {
                toastService.ShowSuccess(successMessage);
            }
            else
            {
                toastService.ShowError("Unexpected empty result");
            }

            return resultWorkingHoursDto;
        }

        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>(options);
        logger.LogError("{Message}", problemDto?.Detail);
        var message = problemDto?.Title ?? "Something went wrong";
        toastService.ShowError(message);
        return null;
    }

    protected virtual async Task<bool> EditAsync<TContent>(string uri, TContent entityToEdit, string successMessage, JsonSerializerOptions options)
        where TContent : class
    {
        var jsonString = JsonSerializer.Serialize(entityToEdit, options);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PatchAsync(uri, httpContent);
        if (response.IsSuccessStatusCode)
        {
            toastService.ShowSuccess(successMessage);
            return true;
        }

        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>(options);
        logger.LogError("{Message}", problemDto?.Detail);
        var message = problemDto?.Title ?? "Something went wrong";
        toastService.ShowError(message);
        return false;
    }

    protected virtual async Task<TResponse?> GetAsync<TResponse>(string uri, string errorMessage, JsonSerializerOptions options)
        where TResponse : class
    {
        try
        {
            return await httpClient.GetFromJsonAsync<TResponse>(uri,options);
        }
        catch (HttpRequestException ex)
        {
            toastService.ShowError(errorMessage);
            logger.LogError("{Message}", ex.Message);
            return null;
        }
    }
}