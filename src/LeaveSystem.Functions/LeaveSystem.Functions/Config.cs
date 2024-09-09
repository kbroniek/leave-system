namespace LeaveSystem.Functions;

using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.LeaveRequests.Canceling;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Domain.LeaveRequests.Getting;
using LeaveSystem.Domain.LeaveRequests.Rejecting;
using LeaveSystem.Domain.LeaveRequests.Searching;
using LeaveSystem.Functions.EventSourcing;
using LeaveSystem.Functions.GraphApi;
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
        public string? LeaveRequestsContainerName { get; set; }
        public string? LeaveTypesContainerName { get; set; }
        public string? LeaveLimitsContainerName { get; set; }
        public string? RolesContainerName { get; set; }
    }

    public static IServiceCollection AddLeaveSystemServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGraphFactory(configuration.GetSection("ManageAzureUsers"));
        var cosmosClientSettings = configuration.Get<CosmosSettings>() ?? throw CreateError(nameof(CosmosSettings));
        return services
            .AddScoped(sp => BuildCosmosDbClient(cosmosClientSettings.CosmosDBConnection ?? throw CreateError(nameof(cosmosClientSettings.CosmosDBConnection))))
            .AddLeaveRequestServices()
            .AddLeaveRequestRepositories(
                cosmosClientSettings.DatabaseName ?? throw CreateError(nameof(cosmosClientSettings.DatabaseName)),
                cosmosClientSettings.LeaveTypesContainerName ?? throw CreateError(nameof(cosmosClientSettings.LeaveTypesContainerName)),
                cosmosClientSettings.LeaveLimitsContainerName ?? throw CreateError(nameof(cosmosClientSettings.LeaveTypesContainerName)),
                cosmosClientSettings.LeaveRequestsContainerName ?? throw CreateError(nameof(cosmosClientSettings.LeaveRequestsContainerName)),
                cosmosClientSettings.RolesContainerName ?? throw CreateError(nameof(cosmosClientSettings.RolesContainerName)))
            .AddLeaveRequestValidators()
            .AddEventSourcing(
                cosmosClientSettings.DatabaseName ?? throw CreateError(nameof(cosmosClientSettings.DatabaseName)),
                cosmosClientSettings.LeaveRequestsContainerName ?? throw CreateError(nameof(cosmosClientSettings.LeaveRequestsContainerName)));
    }

    private static InvalidOperationException CreateError(string propName) => new($"{propName} AppSettings configuration is missing. Check the appsettings.json.");
    private static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        string databaseName,
        string leaveRequestsContainerName) =>
        services
            .AddScoped<ReadService>()
            .AddScoped<WriteService>()
            .AddScoped<EventRepository>(sp => new(
                sp.GetRequiredService<CosmosClient>(),
                sp.GetRequiredService<ILogger<EventRepository>>(),
                databaseName,
                leaveRequestsContainerName)
            )
            .AddScoped((Func<IServiceProvider, IAppendEventRepository>)(sp => sp.GetRequiredService<EventRepository>()))
            .AddScoped((Func<IServiceProvider, IReadEventsRepository>)(sp => sp.GetRequiredService<EventRepository>()));

    private static IServiceCollection AddLeaveRequestServices(this IServiceCollection services) =>
        services
            .AddScoped<CreateLeaveRequestService>()
            .AddScoped<GetLeaveRequestService>()
            .AddScoped<AcceptLeaveRequestService>()
            .AddScoped<RejectLeaveRequestService>()
            .AddScoped<CancelLeaveRequestService>()
            .AddScoped<SearchLeaveRequestService>()
            .AddScoped<EmployeeService>();

    private static IServiceCollection AddLeaveRequestRepositories(
        this IServiceCollection services,
        string databaseName,
        string leaveTypesContainerName,
        string leaveLimitsContainerName,
        string leaveRequestsContainerName,
        string rolesContainerName) =>
        services
            .AddScoped(sp => new CancelledEventsRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    leaveRequestsContainerName
                ))
            .AddScoped<IConnectedLeaveTypesRepository>(sp => new ConnectedLeaveTypesRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    leaveTypesContainerName
                ))
            .AddScoped<IImpositionValidatorRepository>(sp => new ImpositionValidatorRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    leaveRequestsContainerName,
                    sp.GetRequiredService<CancelledEventsRepository>()
                ))
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
                    leaveRequestsContainerName,
                    sp.GetRequiredService<CancelledEventsRepository>()
                ))
            .AddScoped<ISearchLeaveRequestRepository>(sp => new SearchLeaveRequestRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    leaveRequestsContainerName
                ))
            .AddScoped<IRolesRepository>(sp => new RolesRepository(
                    sp.GetRequiredService<CosmosClient>(),
                    databaseName,
                    rolesContainerName
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
