using GoldenEye.Marten.Registration;
using GoldenEye.Registration;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequestDetails;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using Marten;
using Marten.Pagination;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.EventSourcing.LeaveRequests;
internal static class LeaveRequestsConfig
{
    internal static IServiceCollection AddLeaveRequests(this IServiceCollection services) =>
        services.AddMartenEventSourcedRepository<LeaveRequest>()
            .AddCommandHandlers()
            .AddQueryHandlers()
            .AddValidators()
            .AddFactories();

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services) =>
        services
            .AddCommandHandler<CreateLeaveRequest, HandleCreateLeaveRequest>()
            .AddCommandHandler<CreateLeaveRequestOnBehalf, HandleCreateLeaveRequestOnBehalf>()
            .AddCommandHandler<AcceptLeaveRequest, HandleAcceptLeaveRequest>()
            .AddCommandHandler<RejectLeaveRequest, HandleRejectLeaveRequest>()
            .AddCommandHandler<CancelLeaveRequest, HandleCancelLeaveRequest>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services
            .AddQueryHandler<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>, HandleGetLeaveRequests>()
            .AddQueryHandler<GetLeaveRequestDetails, LeaveRequest, HandleGetLeaveRequestDetails>();

    private static IServiceCollection AddValidators(this IServiceCollection services) =>
        services
            .AddScoped<CreateLeaveRequestValidator>();

    private static IServiceCollection AddFactories(this IServiceCollection services) =>
        services
            .AddScoped<LeaveRequestFactory>();


    internal static void ConfigureLeaveRequests(this StoreOptions options)
    {
        // Snapshots
        options.Projections.SelfAggregate<LeaveRequest>();

        // projections
        options.Projections.Add<LeaveRequestShortInfoProjection>();
    }
}

