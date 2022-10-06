using LeaveSystem.Web.Components;
using LeaveSystem.Web.Services;
using Microsoft.JSInterop;

namespace LeaveSystem.Web;

public static class Config
{
    public static void AddLeaveSystemModule(this IServiceCollection services)
    {
        services
            .AddTransient(sp => new TimelineComponent(sp.GetService<IJSRuntime>()))
            .AddTransient<LeaveTypeService>();
    }
}
