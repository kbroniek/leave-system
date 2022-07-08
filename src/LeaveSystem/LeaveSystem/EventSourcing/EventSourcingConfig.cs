
using GoldenEye.Marten.Registration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Diagnostics;

namespace LeaveSystem.EventSourcing;
internal static class EventSourcingConfig
{
    internal static IServiceCollection AddEventSourcing(this IServiceCollection services, string connectionString) =>
        services
            .AddMarten(_ => connectionString, null, null, ServiceLifetime.Scoped)
            .AddLeaveRequests();
}
