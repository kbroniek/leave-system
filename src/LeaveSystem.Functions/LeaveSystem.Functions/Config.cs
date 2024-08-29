namespace LeaveSystem.Functions;

using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Getting;
using LeaveSystem.Functions.EventSourcing;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class Config
{
    private class CosmosClientSettings
    {
        public string? CosmosDBConnection { get; set; }
    }

    internal class EventRepositorySettings
    {
        public string? DatabaseName { get; set; }
        [ConfigurationKeyName("EventsContainerName")]
        public string? ContainerName { get; set; }
    }
    public static IServiceCollection AddLeaveSystemServices(this IServiceCollection services, IConfiguration configuration)
    {
        var cosmosClientSettings = configuration.Get<CosmosClientSettings>() ?? throw new InvalidOperationException("CosmosDB AppSettings configuration is missing. Check the appsettings.json.");
        var eventRepositorySettings = configuration.Get<EventRepositorySettings>() ?? throw new InvalidOperationException("Event repository AppSettings configuration is missing. Check the appsettings.json.");
        return services
            .AddScoped(sp => BuildCosmosDbClient(cosmosClientSettings.CosmosDBConnection ?? throw new InvalidOperationException("CosmosDBConnection AppSettings configuration is missing. Check the appsettings.json.")))
            .AddScoped<CreateLeaveRequestService>()
            .AddScoped<GetLeaveRequestService>()
            .AddScoped<AcceptLeaveRequestService>()
            .AddScoped<ReadService>()
            .AddScoped<WriteService>()
            .AddScoped<EventRepository>(sp => new(
                sp.GetRequiredService<CosmosClient>(),
                sp.GetRequiredService<ILogger<EventRepository>>(),
                eventRepositorySettings)
            )
            .AddScoped<IAppendEventRepository>(sp => sp.GetRequiredService<EventRepository>())
            .AddScoped<IReadEventsRepository>(sp => sp.GetRequiredService<EventRepository>());
    }

    private static CosmosClient BuildCosmosDbClient(string connectionString) =>
        new CosmosClientBuilder(connectionString)
            .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
            .Build();
}
