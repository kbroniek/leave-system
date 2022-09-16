using GoldenEye.Marten.Registration;
using GoldenEye.Registration;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
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
            .AddCommandHandler<AcceptLeaveRequest, HandleAcceptLeaveRequest>()
            .AddCommandHandler<RejectLeaveRequest, HandleRejectLeaveRequest>()
            .AddCommandHandler<CancelLeaveRequest, HandleCancelLeaveRequest>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services
            .AddQueryHandler<GetLeaveRequests, IPagedList<LeaveRequest> , HandleGetLeaveRequest>();

    private static IServiceCollection AddValidators(this IServiceCollection services) =>
        services
            .AddScoped<CreateLeaveRequestValidator>();

    private static IServiceCollection AddFactories(this IServiceCollection services) =>
        services
            .AddScoped<LeaveRequestFactory>();
}

