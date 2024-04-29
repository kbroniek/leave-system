namespace LeaveSystem.Dal;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class Config
{
    public static IServiceCollection AddLeaveSystemModule(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("LeaveSystemCosmosDb");
        return services
            .AddSingleton(new CosmosClient(connectionString, new CosmosClientOptions() { ApplicationName = "LeaveSystem" }));
    }
}
