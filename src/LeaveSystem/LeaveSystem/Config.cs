using GoldenEye.Marten.Registration;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem;

public static class Config
{
    public static IServiceCollection AddLeaveSystemModule(this IServiceCollection services, IConfiguration config)
    {
        var martenConfig = config.GetSection("Marten").Get<MartenConfig>();
        return services
            .AddDbContext<LeaveSystemDbContext>(options => options.UseNpgsql(martenConfig.ConnectionString))
            .AddEventSourcing(config);
    }
}
