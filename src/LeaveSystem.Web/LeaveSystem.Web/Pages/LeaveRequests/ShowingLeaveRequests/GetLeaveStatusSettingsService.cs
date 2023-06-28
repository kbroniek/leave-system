using LeaveSystem.Shared;
using System.Net.Http.Json;
using System.Text.Json;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

public class GetLeaveStatusSettingsService
{
    private readonly HttpClient httpClient;

    public GetLeaveStatusSettingsService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<PagedListResponse<Setting>?> Get()
    {
        var uri = $"odata/settings?$select=Id,Value&$filter=Category eq '{SettingCategoryType.LeaveStatus}'";
        return await httpClient.GetFromJsonAsync<PagedListResponse<Setting>>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public record Setting(string Id, SettingValue Value);
    public record SettingValue(string Color);
}
