using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.Mappers;
internal static class MappersConfig
{
    internal static void AddMappers(this IServiceCollection services)
    {
        services.AddScoped<ILeaveTypeMapper, LeaveTypeMapper>();
    }
}

