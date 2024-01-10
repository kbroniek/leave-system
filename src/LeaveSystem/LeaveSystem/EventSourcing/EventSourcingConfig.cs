using GoldenEye.Backend.Core.DDD.Registration;
using GoldenEye.Backend.Core.Marten.Events.Storage.Custom;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Weasel.Core;

namespace LeaveSystem.EventSourcing;
internal static class EventSourcingConfig
{
    internal static IServiceCollection AddEventSourcing(this IServiceCollection services, MartenConfig config)
    {
        var serializer = new Marten.Services.JsonNetSerializer
        {
            // To change the enum storage policy to store Enum's as strings:
            EnumStorage = EnumStorage.AsString,
            Casing = Casing.CamelCase,
            NonPublicMembersStorage = NonPublicMembersStorage.NonPublicSetters,
        };

        // All other customizations:
        serializer.Customize(_ =>
        {
            // Code directly against a Newtonsoft.Json JsonSerializer
            _.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
            _.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        });
        services.AddMarten(options =>
        {
            options.Connection(config.ConnectionString);
            options.DatabaseSchemaName = config.ReadModelSchema;
            options.Events.DatabaseSchemaName = config.WriteModelSchema;
            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            options.ConfigureLeaveRequests();
            options.ConfigureWorkingHours();
            options.Serializer(serializer);
        }).UseLightweightSessions();
        services.AddEventStore<MartenEventStore>();
        return services
             .AddLeaveRequests()
             .AddWorkingHours();
    }
}

internal record MartenConfig(string ConnectionString, string WriteModelSchema, string ReadModelSchema);
