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
    private class CosmosSettings
    {
        public string? CosmosDBConnection { get; set; }
        public string? DatabaseName { get; set; }
        public string? EventsContainerName { get; set; }
        public string? LeaveTypesContainerName { get; set; }
        public string? LeaveLimitsContainerName { get; set; }
    }

    public static IServiceCollection AddLeaveSystemServices(this IServiceCollection services, IConfiguration configuration)
    {
        var cosmosClientSettings = configuration.Get<CosmosSettings>() ?? throw CreateError(nameof(CosmosSettings));
        return services
            .AddScoped(sp => BuildCosmosDbClient(cosmosClientSettings.CosmosDBConnection ?? throw CreateError(nameof(cosmosClientSettings.CosmosDBConnection))))
            .AddLeaveRequestServices()
            .AddLeaveRequestRepositories(
                cosmosClientSettings.DatabaseName ?? throw CreateError(nameof(cosmosClientSettings.DatabaseName)),
                cosmosClientSettings.LeaveTypesContainerName ?? throw CreateError(nameof(cosmosClientSettings.LeaveTypesContainerName)),
                cosmosClientSettings.LeaveLimitsContainerName ?? throw CreateError(nameof(cosmosClientSettings.LeaveTypesContainerName)),
                cosmosClientSettings.EventsContainerName ?? throw CreateError(nameof(cosmosClientSettings.EventsContainerName)))
            .AddLeaveRequestValidators()
            .AddEventSourcing(
                cosmosClientSettings.DatabaseName ?? throw CreateError(nameof(cosmosClientSettings.DatabaseName)),
                cosmosClientSettings.EventsContainerName ?? throw CreateError(nameof(cosmosClientSettings.EventsContainerName)));
    }

    private static InvalidOperationException CreateError(string propName) => new($"{propName} AppSettings configuration is missing. Check the appsettings.json.");
    private static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        string databaseName,
        string eventsContainerName) =>
        services
            .AddScoped<ReadService>()
            .AddScoped<WriteService>()
            .AddScoped<EventRepository>(sp => new(
                sp.GetRequiredService<CosmosClient>(),
                sp.GetRequiredService<ILogger<EventRepository>>(),
                databaseName,
                eventsContainerName)
            )
            .AddScoped((Func<IServiceProvider, IAppendEventRepository>)(sp => sp.GetRequiredService<EventRepository>()))
            .AddScoped((Func<IServiceProvider, IReadEventsRepository>)(sp => sp.GetRequiredService<EventRepository>()));

    private static IServiceCollection AddLeaveRequestServices(this IServiceCollection services) =>
        services
            .AddScoped<CreateLeaveRequestService>()
            .AddScoped<GetLeaveRequestService>()
            .AddScoped<AcceptLeaveRequestService>();

    private static IServiceCollection AddLeaveRequestRepositories(
        this IServiceCollection services,
        string databaseName,
        string leaveTypesContainerName,
        string leaveLimitsContainerName,
        string eventsContainerName) =>
        services
            .AddScoped<IConnectedLeaveTypesRepository>(sp => new ConnectedLeaveTypesRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    leaveTypesContainerName
                ))
            .AddScoped<IImpositionValidatorRepository, ImpositionValidatorRepository>()
            .AddScoped<ILimitValidatorRepository>(sp => new LimitValidatorRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    leaveLimitsContainerName
                ))
            .AddScoped<ILeaveTypeFreeDaysRepository>(sp => new LeaveTypeFreeDaysRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    leaveTypesContainerName
                ))
            .AddScoped<IUsedLeavesRepository>(sp => new UsedLeavesRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    eventsContainerName
                ));


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
