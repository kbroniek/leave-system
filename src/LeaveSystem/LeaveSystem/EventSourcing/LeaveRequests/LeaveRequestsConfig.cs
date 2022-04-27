using GoldenEye.Marten.Registration;
using GoldenEye.Registration;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.EventSourcing;
internal static class LeaveRequestsConfig
{
    internal static void AddLeaveRequests(this IServiceCollection services, string connectionString)
    {
        services.AddMartenEventSourcedRepository<LeaveRequest>()
            .AddCommandHandlers()
            .AddQueryHandlers();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        return services
            .AddCommandHandler<CreateLeaveRequest, HandleCreateLeaveRequest>();
    }

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        return services;
    }
}

