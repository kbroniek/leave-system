using FluentValidation;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem;

public static class Config
{
    public static IServiceCollection AddLeaveSystemModule(this IServiceCollection services, IConfiguration config)
    {
        var martenConfig = config.GetSection("Marten").Get<MartenConfig>() ?? throw new InvalidOperationException("Marten configuration is missing. Check the appsettings.json.");
        return services
            .AddDbContext<LeaveSystemDbContext>(options => options.UseNpgsql(martenConfig.ConnectionString))
            .AddEventSourcing(martenConfig);
    }

    public static IServiceCollection AddValidators(this IServiceCollection services) =>
        services.AddScoped<IValidator<UserLeaveLimit>, UserLeaveLimitValidator>()
            .AddScoped<IValidator<LeaveType>, LeaveTypeValidator>()
            .AddScoped<IValidator<Setting>, SettingValidator>();
}
