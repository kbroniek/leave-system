
using GoldenEye.Marten.Registration;
using LeaveSystem.EventSourcing.LeaveRequests;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.EventSourcing;
internal static class EventSourcingConfig
{
    internal static IServiceCollection AddEventSourcing(this IServiceCollection services, string connectionString) =>
        services
            .AddMarten(_ => connectionString, null, null, ServiceLifetime.Scoped)
            .AddLeaveRequests();
}
