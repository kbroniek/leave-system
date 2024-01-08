using GoldenEye.Backend.Core.DDD.Registration;
using GoldenEye.Backend.Core.Marten.Events.Storage.Custom;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Core;

namespace LeaveSystem.EventSourcing;
internal static class EventSourcingConfig
{
    internal static IServiceCollection AddEventSourcing(this IServiceCollection services, MartenConfig config)
    {
        services.AddMarten(options =>
        {
            options.Connection(config.ConnectionString);
            options.DatabaseSchemaName = config.ReadModelSchema;
            options.Events.DatabaseSchemaName = config.WriteModelSchema;
            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            options.ConfigureLeaveRequests();
            options.ConfigureWorkingHours();
        }).UseLightweightSessions();
        services.AddEventStore<MartenEventStore>();
        return services
             .AddLeaveRequests()
             .AddWorkingHours();
    }
}

internal record MartenConfig(string ConnectionString, string WriteModelSchema, string ReadModelSchema);
