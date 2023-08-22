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
    public async Task<IEnumerable<Setting>> Get()
    {
        // TODO: change to odata when it will be fixed https://github.com/OData/AspNetCoreOData/issues/586, https://stackoverflow.com/questions/76303065/odata-always-returns-for-entity-jsondocument-property
        // var uri = $"odata/settings?$select=Id,Value&$filter=Category eq '{SettingCategoryType.LeaveStatus}'";
        var uri = $"api/settings?$filter=Category eq '{SettingCategoryType.LeaveStatus}'";
        var result = await httpClient.GetFromJsonAsync<IEnumerable<Setting>>(uri, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return result ?? Enumerable.Empty<Setting>();
    }

    public record Setting(string Id, SettingValue Value);
    public record SettingValue(string Color);
}
