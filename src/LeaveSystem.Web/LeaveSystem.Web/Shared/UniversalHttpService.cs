using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.Toast.Services;
using LeaveSystem.Shared;

namespace LeaveSystem.Web.Shared;

public class UniversalHttpService
{
    private const string PutMethodName = "Put";
    private const string PatchMethodName = "Patch";

    private readonly HttpClient httpClient;
    private readonly IToastService toastService;
    private readonly ILogger<UniversalHttpService> logger;

    public UniversalHttpService(HttpClient httpClient, IToastService toastService, ILogger<UniversalHttpService> logger)
    {
        this.httpClient = httpClient;
        this.toastService = toastService;
        this.logger = logger;
    }

    public virtual async Task<TResponse?> PostAsync<TContent, TResponse>(string uri, TContent entityToAdd,
        string successMessage, JsonSerializerOptions options)
    {
        var jsonString = JsonSerializer.Serialize(entityToAdd, options);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        try
        {
            var response = await this.httpClient.PostAsync(uri, httpContent);
            if (response.IsSuccessStatusCode)
            {
                var resultDto = await response.Content.ReadFromJsonAsync<TResponse>(options);
                this.toastService.ShowSuccess(successMessage);
                return resultDto;
            }

            await this.HandleProblemAsync(options, response);
        }
        catch (Exception e)
        {
            this.toastService.ShowError("Error occured while adding");
            this.logger.LogError(e, "Error occured while adding resource of type {Type}", typeof(TContent));
        }

        return default;
    }

    private async Task HandleProblemAsync(JsonSerializerOptions options, HttpResponseMessage response)
    {
        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDto>(options);
        this.logger.LogError("{Message}", problemDto?.Detail);
        var message = problemDto?.Title ?? "Something went wrong";
        this.toastService.ShowError(message);
    }

    public virtual Task<bool> PatchAsync<TContent>(string uri, TContent entityToEdit, string successMessage,
        JsonSerializerOptions options) =>
        this.EditAsync(uri, entityToEdit, successMessage, PatchMethodName, options);

    public virtual Task<bool> PutAsync<TContent>(string uri, TContent entityToEdit, string successMessage,
        JsonSerializerOptions options) =>
        this.EditAsync(uri, entityToEdit, successMessage, PutMethodName, options);

    private async Task<bool> EditAsync<TContent>(string uri, TContent entityToEdit, string successMessage, string editMethodName, JsonSerializerOptions options)
    {
        var jsonString = JsonSerializer.Serialize(entityToEdit, options);
        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        try
        {
            var response = await (editMethodName switch
            {
                PutMethodName => this.httpClient.PutAsync(uri, httpContent),
                PatchMethodName => this.httpClient.PatchAsync(uri, httpContent),
                _ => throw new ArgumentOutOfRangeException(nameof(editMethodName), editMethodName, "Bad edit method name")
            });
            if (response.IsSuccessStatusCode)
            {
                this.toastService.ShowSuccess(successMessage);
                return true;
            }
            await this.HandleProblemAsync(options, response);
        }
        catch (Exception e)
        {
            this.toastService.ShowError("Error occured while editing");
            this.logger.LogError(e, "Error occured while editing resource of type {Type}", typeof(TContent));
        }
        return false;
    }

    public virtual async Task<TResponse?> GetAsync<TResponse>(string uri, string errorMessage,
        JsonSerializerOptions options)
    {
        try
        {
            return await this.httpClient.GetFromJsonAsync<TResponse>(uri, options);
        }
        catch (HttpRequestException ex)
        {
            this.toastService.ShowError(errorMessage);
            this.logger.LogError(ex, "Error occured while getting resource of type {Type}", typeof(TResponse));
            return default;
        }
    }

    public virtual async Task<bool> DeleteAsync(string uri, string successMessage, JsonSerializerOptions options)
    {
        try
        {
            var response = await this.httpClient.DeleteAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                this.toastService.ShowSuccess(successMessage);
                return true;
            }
            await this.HandleProblemAsync(options, response);
        }
        catch (Exception e)
        {
            this.toastService.ShowError("Error occured while deleting");
            this.logger.LogError(e, "Error occured while deleting resource");
        }
        return false;
    }
}
