using GoldenEye.Backend.Core.Marten.Registration;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.EventSourcing;
internal static class EventSourcingConfig
{
    internal static IServiceCollection AddEventSourcing(this IServiceCollection services, MartenConfig config)
    {
        services.AddMarten(_ => config.ConnectionString, options =>
            {
                options.DatabaseSchemaName = config.ReadModelSchema;
                options.Events.DatabaseSchemaName = config.WriteModelSchema;
                options.ConfigureLeaveRequests();
                options.ConfigureWorkingHours();
            });
        return services
             .AddLeaveRequests()
             .AddWorkingHours();
    }
}

internal record MartenConfig(string ConnectionString, string WriteModelSchema, string ReadModelSchema);
