namespace LeaveSystem.Functions;

using LeaveSystem.Functions.EventSourcing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class Config
{
    private class CosmosClientSettings
    {
        public required string CosmosDBConnection { get; set; }
    }

    internal class EventRepositorySettings
    {
        public required string DatabaseName { get; set; }
        [ConfigurationKeyName("EventsContainerName")]
        public required string ContainerName { get; set; }
    }
    public static IServiceCollection AddLeaveSystemServcies(this IServiceCollection services, IConfiguration configuration)
    {
        var cosmosClientSettings = configuration.Get<CosmosClientSettings>() ?? throw new InvalidOperationException("CosmosDB AppSettings configuration is missing. Check the appsettings.json.");
        var eventRepositorySettings = configuration.Get<EventRepositorySettings>() ?? throw new InvalidOperationException("Event repository AppSettings configuration is missing. Check the appsettings.json.");
        return services
            .AddScoped<CosmosClient>(sp => new(cosmosClientSettings.CosmosDBConnection))
            .AddScoped<EventRepository>(sp => new(
                sp.GetRequiredService<CosmosClient>(),
                sp.GetRequiredService<ILogger<EventRepository>>(),
                eventRepositorySettings)
            );
    }
}
