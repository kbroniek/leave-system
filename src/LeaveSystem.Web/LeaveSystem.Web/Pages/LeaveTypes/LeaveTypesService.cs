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
        var leaveTypes = await httpClient.GetFromJsonAsync<ODataResponse<IEnumerable<LeaveTypeDto>>>("odata/LeaveTypes?$select=Id,BaseLeaveTypeId,Name,Properties&$orderby=Order asc");
        return leaveTypes?.Data ?? Enumerable.Empty<LeaveTypeDto>();
    }

    public record class LeaveTypeDto(Guid Id, Guid? BaseLeaveTypeId, string Name, LeaveTypeProperties Properties);
    public record class LeaveTypeProperties(string? Color, bool? IsDefault);
}

