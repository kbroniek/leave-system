using GoldenEye.Marten.Registration;
using GoldenEye.Registration;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.EventSourcing;
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
            .AddCommandHandler<CreateLeaveRequest, HandleCreateLeaveRequest>();

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services) =>
        services;

    private static IServiceCollection AddValidators(this IServiceCollection services) =>
        services
            .AddScoped<CreateLeaveRequestValidator>();

    private static IServiceCollection AddFactories(this IServiceCollection services) =>
        services
            .AddScoped<LeaveRequestFactory>();
}

