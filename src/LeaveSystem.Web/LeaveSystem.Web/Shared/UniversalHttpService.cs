using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Logger;
using LeaveSystem.Shared.WorkingHours;
using Microsoft.JSInterop;

namespace LeaveSystem.Web.Shared;

public class UniversalHttpService
{
    private readonly HttpClient httpClient;
    private readonly IToastService toastService;
    private readonly ILogger<UniversalHttpService> logger;
    //Todo: dodać jako klasę wstrzykiwaną, a nie taką po której się dziedziczy
    public UniversalHttpService(HttpClient httpClient, IToastService toastService, ILogger<UniversalHttpService> logger)
    {
        this.httpClient = httpClient;
        this.toastService = toastService;
        this.logger = logger;
    }

    internal virtual async Task<TResponse?> AddAsync<TContent, TResponse>(string uri, TContent entityToAdd,
        string successMessage, JsonSerializerOptions options)
        where TContent : class
        where TResponse : class
    {
        var jsonString = JsonSerializer.Serialize(entityToAdd, options);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(uri, httpContent);
        if (response.IsSuccessStatusCode)
        {
            return await DeserializeResponseContent<TResponse>(response, successMessage, options);
        }
        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>(options);
        logger.LogError("{Message}", problemDto?.Detail);
        var message = problemDto?.Title ?? "Something went wrong";
        toastService.ShowError(message);
        return null;
    }

    internal async Task<TResponse?> DeserializeResponseContent<TResponse> (
        HttpResponseMessage response, string successMessage, JsonSerializerOptions options) 
        where TResponse : class
    {
        try
        {
            var resultWorkingHoursDto = await response.Content.ReadFromJsonAsync<TResponse>(options);
            if (resultWorkingHoursDto is not null)
            {
                toastService.ShowSuccess(successMessage);
                return resultWorkingHoursDto;
            }
            toastService.ShowError("Unexpected empty result");
        }
        catch (Exception e)
        {
            toastService.ShowError("Error occured while adding");
            logger.LogException(e);
        }
        return null;
    }

    internal virtual async Task<bool> EditAsync<TContent>(string uri, TContent entityToEdit, string successMessage, JsonSerializerOptions options)
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

    internal  virtual async Task<TResponse?> GetAsync<TResponse>(string uri, string errorMessage, JsonSerializerOptions options)
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