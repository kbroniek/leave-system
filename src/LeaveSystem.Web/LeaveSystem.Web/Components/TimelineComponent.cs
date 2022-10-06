using Ardalis.GuardClauses;
using LeaveSystem.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LeaveSystem.Web.Components;

public class TimelineComponent
{
    private readonly IJSRuntime jSRuntime;

    public TimelineComponent(IJSRuntime? jSRuntime)
    {
        this.jSRuntime = Guard.Against.Nill(jSRuntime);
    }

    public async Task CreateAsync(string? container)
    {
        await jSRuntime.InvokeVoidAsync("TimelineWrapper.create", container);
    }
}

