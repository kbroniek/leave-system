using Ardalis.GuardClauses;
using LeaveSystem.Shared;
using Microsoft.JSInterop;
using static LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests.ShowLeaveRequests;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

public class TimelineComponent
{
    private readonly IJSRuntime jSRuntime;

    public TimelineComponent(IJSRuntime? jSRuntime)
    {
        this.jSRuntime = Guard.Against.Nill(jSRuntime);
    }

    public async Task CreateAsync(string? container,
        IEnumerable<FederatedUser>? users,
        IEnumerable<LeaveRequestSummary>? leaveRequests,
        DateTimeOffset minDate,
        DateTimeOffset maxDate)
    {
        await jSRuntime.InvokeVoidAsync("TimelineWrapper.create", container, users, leaveRequests, minDate, maxDate);
    }
}

