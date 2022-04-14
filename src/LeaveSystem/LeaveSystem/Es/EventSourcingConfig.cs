using GoldenEye.Marten.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.Es;
internal static class EventSourcingConfig
{
    internal static void AddEventSourcing(this IServiceCollection services)
    {
        services.AddMartenEventSourcedRepository<LeaveRequest>();
    }
}

