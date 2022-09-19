
using GoldenEye.Marten.Registration;
using LeaveSystem.EventSourcing.LeaveRequests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.EventSourcing;
internal static class EventSourcingConfig
{
    internal static IServiceCollection AddEventSourcing(this IServiceCollection services, IConfiguration config) =>
        services
            .AddMarten(config, options =>
            {
                options.ConfigureLeaveRequests();
            })
            .AddLeaveRequests();
}
