using LeaveSystem.Db.Entities;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LeaveSystem.Api.Controllers;

public static class ControllersConfig
{
    public static void AddDeltaValidators(this IServiceCollection services)
    {
        services.AddScoped<DeltaValidator<UserLeaveLimit>>(x =>
                new(
                    nameof(UserLeaveLimit.LeaveTypeId),
                    nameof(UserLeaveLimit.AssignedToUserId),
                    nameof(UserLeaveLimit.Id)))
            .AddScoped<DeltaValidator<LeaveType>>(x =>
                new(nameof(UserLeaveLimit.Id)))
            .AddScoped<DeltaValidator<Setting>>(x =>
                new(nameof(UserLeaveLimit.Id)));
    }

    public static void AddCrudService(this IServiceCollection services)
    {
        services.AddScoped<GenericCrudService<UserLeaveLimit, Guid>>()
            .AddScoped<GenericCrudService<LeaveType, Guid>>()
            .AddScoped<GenericCrudService<Setting, string>>();
    }
}