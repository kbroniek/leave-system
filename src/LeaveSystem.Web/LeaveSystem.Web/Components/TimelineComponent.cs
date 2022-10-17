using Ardalis.GuardClauses;
using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using Microsoft.JSInterop;

namespace LeaveSystem.Web.Components;

public class TimelineComponent
{
    private readonly IJSRuntime jSRuntime;

    public TimelineComponent(IJSRuntime? jSRuntime)
    {
        this.jSRuntime = Guard.Against.Nill(jSRuntime);
    }

    public async Task CreateAsync(string? container,
        IEnumerable<FederatedUser>? users,
        IEnumerable<LeaveRequestShortInfo>? leaveRequests,
        DateTimeOffset minDate,
        DateTimeOffset maxDate)
    {
        await jSRuntime.InvokeVoidAsync("TimelineWrapper.create", container, users, leaveRequests, minDate, maxDate);
    }
}

