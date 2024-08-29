namespace LeaveSystem.Functions;

using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Domain.LeaveRequests.Getting;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.LeaveRequests.Repositories;
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
            .AddLeaveRequestServices()
            .AddLeaveRequestRepositories()
            .AddLeaveRequestValidators()
            .AddEventSourcing(eventRepositorySettings);
    }

    private static IServiceCollection AddEventSourcing(this IServiceCollection services, EventRepositorySettings eventRepositorySettings) =>
        services
            .AddScoped<ReadService>()
            .AddScoped<WriteService>()
            .AddScoped<EventRepository>(sp => new(
                sp.GetRequiredService<CosmosClient>(),
                sp.GetRequiredService<ILogger<EventRepository>>(),
                eventRepositorySettings)
            )
            .AddScoped((Func<IServiceProvider, IAppendEventRepository>)(sp => sp.GetRequiredService<EventRepository>()))
            .AddScoped((Func<IServiceProvider, IReadEventsRepository>)(sp => sp.GetRequiredService<EventRepository>()));

    private static IServiceCollection AddLeaveRequestServices(this IServiceCollection services) =>
        services
            .AddScoped<CreateLeaveRequestService>()
            .AddScoped<GetLeaveRequestService>()
            .AddScoped<AcceptLeaveRequestService>();

    private static IServiceCollection AddLeaveRequestRepositories(this IServiceCollection services) =>
        services
            .AddScoped<IConnectedLeaveTypesRepository, ConnectedLeaveTypesRepository>()
            .AddScoped<IImpositionValidatorRepository, ImpositionValidatorRepository>()
            .AddScoped<IImpositionValidatorRepository, ImpositionValidatorRepository>()
            .AddScoped<ILeaveTypeFreeDaysRepository, LeaveTypeFreeDaysRepository>()
            .AddScoped<IUsedLeavesRepository, UsedLeavesRepository>();


    private static IServiceCollection AddLeaveRequestValidators(this IServiceCollection services) =>
        services
            .AddScoped<BasicValidator>()
            .AddScoped<ImpositionValidator>()
            .AddScoped<LimitValidator>()
            .AddScoped<CreateLeaveRequestValidator>();

    private static CosmosClient BuildCosmosDbClient(string connectionString) =>
        new CosmosClientBuilder(connectionString)
            .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
            .Build();
}
