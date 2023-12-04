using LeaveSystem.Web.Pages.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.CancellingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.RejectingLeaveRequest;
using System.Net.Http.Json;
using System.Text.Json;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequestDetails;

public class ChangeStatusService
{
    private readonly HttpClient httpClient;

    public ChangeStatusService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task Accept(string leaveRequestId, string? remarks)
    {
        var uri = $"api/leaveRequests/{leaveRequestId}/accept";
        await httpClient.PutAsJsonAsync(uri, new AcceptLeaveRequestDto { Remarks = remarks }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public async Task Reject(string leaveRequestId, string? remarks)
    {
        var uri = $"api/leaveRequests/{leaveRequestId}/reject";
        await httpClient.PutAsJsonAsync(uri, new RejectLeaveRequestDto { Remarks = remarks }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
    public async Task Cancel(string leaveRequestId, string? remarks)
    {
        var uri = $"api/leaveRequests/{leaveRequestId}/cancel";
        await httpClient.PutAsJsonAsync(uri, new CancelLeaveRequestDto { Remarks = remarks }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
