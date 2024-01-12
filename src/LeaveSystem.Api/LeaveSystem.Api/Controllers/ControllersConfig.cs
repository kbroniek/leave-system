using LeaveSystem.Db.Entities;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeaveSystem.Api.Controllers;

public static class ControllersConfig
{
    public static IServiceCollection AddODataControllersServices(this IServiceCollection services)
    {
        return services.AddDeltaValidators()
            .AddCrudService()
            .AddEntityServices();
    }
    private static IServiceCollection AddDeltaValidators(this IServiceCollection services)
    {
        return services.AddScoped<DeltaValidator<UserLeaveLimit>>(x =>
                new(
                    nameof(UserLeaveLimit.LeaveTypeId),
                    nameof(UserLeaveLimit.AssignedToUserId),
                    nameof(UserLeaveLimit.Id)))
            .AddScoped<DeltaValidator<LeaveType>>(x =>
                new(nameof(UserLeaveLimit.Id)))
            .AddScoped<DeltaValidator<Setting>>(x =>
                new(nameof(UserLeaveLimit.Id)));
    }

    private static IServiceCollection AddCrudService(this IServiceCollection services)
    {
        return services.AddScoped<GenericCrudService<UserLeaveLimit, Guid>>()
            .AddScoped<GenericCrudService<LeaveType, Guid>>()
            .AddScoped<GenericCrudService<Setting, string>>();
    }

    private static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        return services.AddScoped<NeighbouringLimitsService>();
    }
}