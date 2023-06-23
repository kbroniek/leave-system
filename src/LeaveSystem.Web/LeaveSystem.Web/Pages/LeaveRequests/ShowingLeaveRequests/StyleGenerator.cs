using Ardalis.GuardClauses;
using LeaveSystem.Shared;
using Microsoft.JSInterop;

namespace LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;

public class StyleGenerator
{
    private readonly IJSRuntime jSRuntime;

    public StyleGenerator(IJSRuntime? jSRuntime)
    {
        this.jSRuntime = Guard.Against.Nill(jSRuntime);
    }

    public async Task CreateAsync(string style)
    {
        await jSRuntime.InvokeVoidAsync("StyleGenerator.create", style);
    }
}
