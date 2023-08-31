using GoldenEye.Marten.Registration;
using GoldenEye.Registration;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.EventSourcing.WorkingHours;

public static class WorkingHoursConfig
{
        
    internal static IServiceCollection AddWorkingHours(this IServiceCollection services) =>
        services.AddMartenEventSourcedRepository<WorkingHours>()
            .AddCommandHandlers()
            .AddQueryHandlers()
            .AddLeaveRequestValidators()
            .AddLeaveRequestFactories();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services
            .AddCommandHandler<CreateWorkingHours, HandleCreateWorkingHours>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services
            .AddQueryHandler<GetWorkingHoursByUserId, WorkingHours, HandleGetWorkingHoursByUserId>();
    
    private static IServiceCollection AddLeaveRequestValidators(this IServiceCollection services) =>
        services
            .AddScoped<CreateWorkingHoursValidator>();

    private static IServiceCollection AddLeaveRequestFactories(this IServiceCollection services) =>
        services
            .AddScoped<WorkingHoursFactory>();
    
    internal static void ConfigureWorkingHours(this StoreOptions options)
    {
        // Snapshots
        options.Projections.SelfAggregate<WorkingHours>();
    }
}