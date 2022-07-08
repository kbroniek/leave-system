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
        string connectionString = config.GetConnectionString("PostgreSQL");
        services
            .AddDbContext<LeaveSystemDbContext>(options => options.UseNpgsql(connectionString))
            .AddEventSourcing(connectionString)
            .AddServices();
    }

    private static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddScoped<WorkingHoursService>();
}
