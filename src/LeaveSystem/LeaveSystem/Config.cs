using FluentValidation;
using GoldenEye.Backend.Core.Marten.Registration.Cusom;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Domain.Employees.GettingEmployees;
using LeaveSystem.EventSourcing;
using LeaveSystem.Shared;
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
            .AddEventSourcing(martenConfig)
            .AddValidators()
            .AddQueryHandlers();
    }

    public static IServiceCollection AddValidators(this IServiceCollection services) =>
        services.AddScoped<IValidator<UserLeaveLimit>, UserLeaveLimitValidator>()
            .AddScoped<IValidator<LeaveType>, LeaveTypeValidator>()
            .AddScoped<IValidator<Setting>, SettingValidator>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services
            .AddQueryHandler<GetEmployees, IEnumerable<FederatedUser>, HandleGetEmployees>()
            .AddQueryHandler<GetSingleEmployee, FederatedUser, HandleGetSingleEmployee>();
}
