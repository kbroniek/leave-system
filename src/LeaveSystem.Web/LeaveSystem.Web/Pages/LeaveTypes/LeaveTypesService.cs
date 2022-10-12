using System.Net.Http.Json;

namespace LeaveSystem.Web.Pages.LeaveTypes;

public class LeaveTypesService
{
    private readonly HttpClient httpClient;

    public LeaveTypesService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypes()
    {
        var leaveTypes = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<LeaveTypeDto>>>("odata/LeaveTypes?$select=Id,Name&$orderby=Order asc");
        return leaveTypes?.Data ?? Enumerable.Empty<LeaveTypeDto>();
    }


    public record class LeaveTypeDto(Guid Id, string Name);
}

