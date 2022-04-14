using GoldenEye.Marten.Registration;
using LeaveSystem.Db;
using LeaveSystem.Es;
using LeaveSystem.Mappers;
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
        services.AddDbContext<LeaveSystemDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services.AddMarten(_ => connectionString);
        services.AddEventSourcing();
        services.AddServices();
        services.AddMappers();
        //services.AddMaintainance();
    }
}
