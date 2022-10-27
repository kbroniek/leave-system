using LeaveSystem.Shared.WorkingHours;
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

    public async Task<WorkingHoursCollection> GetWorkingHours(IEnumerable<string> userEmails, DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        var query = new
        {
            UserEmails = userEmails,
            DateFrom = dateFrom,
            dateTo = dateTo,
        };
        var uri = query.CreateQueryString($"api/workingHours");
        var workingHours = await httpClient.GetFromJsonAsync<WorkingHoursCollectionDto>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return new WorkingHoursCollection(workingHours?.WorkingHours ?? Enumerable.Empty<WorkingHoursModel>());
    }

    public async Task<WorkingHoursCollection> GetUserWorkingHours(string userEmail, DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        var query = new
        {
            DateFrom = dateFrom,
            dateTo = dateTo,
        };
        var uri = query.CreateQueryString($"api/workingHours/{userEmail}");
        var workingHours = await httpClient.GetFromJsonAsync<WorkingHoursCollectionDto>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return new WorkingHoursCollection(workingHours?.WorkingHours ?? Enumerable.Empty<WorkingHoursModel>());
    }
}

