using LeaveSystem.Db;
using LeaveSystem.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LeaveSystem;

public static class Config
{
    public static void AddLeaveSystemModule(this IServiceCollection services, IConfiguration config)
    {
        string connectionString = config.GetConnectionString("PostgreSQL");
        services.AddDbContext<LeaveSystemDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services.AddEventSourcing(connectionString);
    }
}
