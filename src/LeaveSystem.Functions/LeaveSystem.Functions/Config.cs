namespace LeaveSystem.Functions;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal static class Config
{
    public static IServiceCollection AddLeaveSystemServcies(this IServiceCollection services) =>
        services.AddScoped<CosmosClient>(sp => new(sp.GetRequiredService<IConfiguration>()["CosmosDBConnection"]));
}
