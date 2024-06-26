using GoldenEye.Backend.Core.Marten.Registration.Cusom;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.ModyfingWorkingHours;
using Marten;
using Marten.Pagination;
using Microsoft.Extensions.DependencyInjection;
using static LeaveSystem.EventSourcing.WorkingHours.WorkingHours;

namespace LeaveSystem.EventSourcing.WorkingHours;

public static class WorkingHoursConfig
{
    internal static IServiceCollection AddWorkingHours(this IServiceCollection services)
    {
        services.AddMartenEventSourcedRepository<WorkingHours>();
        return services.AddCommandHandlers()
            .AddQueryHandlers()
            .AddLeaveRequestValidators()
            .AddLeaveRequestFactories();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services
            .AddCommandHandler<CreateWorkingHours, HandleCreateWorkingHours>()
            .AddCommandHandler<ModifyWorkingHours, HandleModifyWorkingHours>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services
            .AddQueryHandler<GetCurrentWorkingHoursByUserId, WorkingHours, HandleGetWorkingHoursByUserId>()
            .AddQueryHandler<GetWorkingHours, IPagedList<WorkingHours>, HandleGetWorkingHours>();

    private static IServiceCollection AddLeaveRequestValidators(this IServiceCollection services) =>
        services;

    private static IServiceCollection AddLeaveRequestFactories(this IServiceCollection services) =>
        services
            .AddScoped<WorkingHoursFactory>();

    internal static void ConfigureWorkingHours(this StoreOptions options) =>
        // Projections
        options.Projections.Add<WorkingHoursProjection>(Marten.Events.Projections.ProjectionLifecycle.Inline);
}
