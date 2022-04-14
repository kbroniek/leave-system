using GoldenEye.Marten.Registration;
using LeaveSystem.Services.LeaveType;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.Services;
internal static class ServicesConfig
{
    internal static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<LeaveTypeService>();
    }
}

