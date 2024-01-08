using GoldenEye.Backend.Core.DDD.Commands;
using GoldenEye.Backend.Core.DDD.Queries;
using GoldenEye.Backend.Core.Marten.Registration;
using GoldenEye.Shared.Core.Extensions.DependencyInjection;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequestDetails;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using Marten;
using Marten.Pagination;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.EventSourcing.LeaveRequests;
internal static class LeaveRequestsConfig
{
    internal static IServiceCollection AddLeaveRequests(this IServiceCollection services)
    {
        services.AddMartenEventSourcedRepository<LeaveRequest>();
        return services.AddLeaveRequestCommandHandlers()
            .AddLeaveRequestQueryHandlers()
            .AddLeaveRequestValidators()
            .AddLeaveRequestServices()
            .AddLeaveRequestFactories();
    }

    private static IServiceCollection AddLeaveRequestCommandHandlers(this IServiceCollection services) =>
        services
            .AddCommandHandler<CreateLeaveRequest, HandleCreateLeaveRequest>()
            .AddCommandHandler<CreateLeaveRequestOnBehalf, HandleCreateLeaveRequestOnBehalf>()
            .AddCommandHandler<AcceptLeaveRequest, HandleAcceptLeaveRequest>()
            .AddCommandHandler<RejectLeaveRequest, HandleRejectLeaveRequest>()
            .AddCommandHandler<CancelLeaveRequest, HandleCancelLeaveRequest>();

    private static IServiceCollection AddLeaveRequestQueryHandlers(this IServiceCollection services) =>
        services
            .AddQueryHandler<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>, HandleGetLeaveRequests>()
            .AddQueryHandler<GetLeaveRequestDetails, LeaveRequest, HandleGetLeaveRequestDetails>();

    private static IServiceCollection AddLeaveRequestValidators(this IServiceCollection services) =>
        services
            .AddScoped<CreateLeaveRequestValidator>()
            .AddScoped<BasicValidator>()
            .AddScoped<ImpositionValidator>()
            .AddScoped<LimitValidator>();

    private static IServiceCollection AddLeaveRequestServices(this IServiceCollection services) =>
        services
            .AddScoped<UsedLeavesService>()
            .AddScoped<LeaveLimitsService>()
            .AddScoped<ConnectedLeaveTypesService>();

    private static IServiceCollection AddLeaveRequestFactories(this IServiceCollection services) =>
        services
            .AddScoped<LeaveRequestFactory>();


    internal static void ConfigureLeaveRequests(this StoreOptions options)
    {
        // Snapshots
        options.Projections.LiveStreamAggregation<LeaveRequest>();

        // projections
        options.Projections.Add<LeaveRequestShortInfoProjection>(Marten.Events.Projections.ProjectionLifecycle.Inline);
    }

    public static IServiceCollection AddCommandHandler<TCommand, TCommandHandler>(
        this IServiceCollection services, ServiceLifetime withLifetime = ServiceLifetime.Transient)
        where TCommand : ICommand
        where TCommandHandler : class, ICommandHandler<TCommand> =>
            services.Add<TCommandHandler>(withLifetime)
                .Add<IRequestHandler<TCommand, Unit>>(sp => sp.GetService<TCommandHandler>()!, withLifetime)
                .Add<ICommandHandler<TCommand>>(sp => sp.GetService<TCommandHandler>()!, withLifetime);

    public static IServiceCollection AddQueryHandler<TQuery, TResponse, TQueryHandler>(
        this IServiceCollection services, ServiceLifetime withLifetime = ServiceLifetime.Transient)
        where TQuery : IQuery<TResponse>
        where TQueryHandler : class, IQueryHandler<TQuery, TResponse> =>
            services.Add<TQueryHandler>(withLifetime)
                .Add<IRequestHandler<TQuery, TResponse>>(sp => sp.GetService<TQueryHandler>()!, withLifetime)
                .Add<IQueryHandler<TQuery, TResponse>>(sp => sp.GetService<TQueryHandler>()!, withLifetime);
}

