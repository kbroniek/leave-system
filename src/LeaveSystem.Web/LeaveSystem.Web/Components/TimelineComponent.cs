using Ardalis.GuardClauses;
using LeaveSystem.Db;
using LeaveSystem.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequest.ShowLeaveRequest;

namespace LeaveSystem.Web.Components;

public class TimelineComponent
{
    private readonly IJSRuntime jSRuntime;

    public TimelineComponent(IJSRuntime? jSRuntime)
    {
        this.jSRuntime = Guard.Against.Nill(jSRuntime);
    }

    public async Task CreateAsync(string? container, IEnumerable<FederatedUser>? users, IEnumerable<LeaveRequestShortInfo>? leaveRequests)
    {
        await jSRuntime.InvokeVoidAsync("TimelineWrapper.create", container, users, leaveRequests);
    }
}

