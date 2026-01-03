namespace LeaveSystem.Functions.GraphApi;

using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class Config
{
    public class AppSettings
    {
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? Secret { get; set; }
        public string[]? Scopes { get; set; }
    }

    public static IServiceCollection AddGraphFactory(this IServiceCollection services, IConfigurationSection configuration)
    {
        var settings = configuration.Get<AppSettings>() ?? throw new InvalidOperationException("Azure AppSettings configuration is missing. Check the appsettings.json.");
        return services
            .AddScoped<IGraphClientFactory, GraphClientFactory>(_ => GraphClientFactory.Create(
                settings.TenantId, settings.ClientId,
                settings.Secret, settings.Scopes))
            .AddScoped<IGetUserRepository, GetUserRepository>()
            .AddScoped<UpdateUserRepository>();
    }
}
