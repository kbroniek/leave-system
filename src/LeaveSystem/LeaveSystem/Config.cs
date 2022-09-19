using GoldenEye.Marten.Registration;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing;
using LeaveSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem;

public static class Config
{
    public static void AddLeaveSystemModule(this IServiceCollection services, IConfiguration config)
    {
        var martenConfig = config.GetSection("Marten").Get<MartenConfig>();
        services
            .AddDbContext<LeaveSystemDbContext>(options => options.UseNpgsql(martenConfig.ConnectionString))
            .AddEventSourcing(config)
            .AddServices();
    }

    private static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddScoped<WorkingHoursService>();
}
