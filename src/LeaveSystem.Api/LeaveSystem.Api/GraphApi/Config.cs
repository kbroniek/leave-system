using LeaveSystem.Api.Endpoints.Employees;
using LeaveSystem.Api.Endpoints.Users;

namespace LeaveSystem.Api.GraphApi;

public static class Config
{
    public class AppSettings
    {
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? Secret { get; set; }
        public string[]? Scopes { get; set; }
        public string? B2cExtensionAppClientId { get; set; }
    }

    public static void AddGraphFactory(this IServiceCollection services, IConfigurationSection configuration)
    {
        var settings = configuration.Get<AppSettings>();
        services
            .AddScoped<IGraphClientFactory, GraphClientFactory>(_ => GraphClientFactory.Create(settings.TenantId,
                                            settings.ClientId,
                                            settings.Secret,
                                            settings.Scopes))
            .AddScoped(_ => RoleAttributeNameResolver.Create(settings.B2cExtensionAppClientId))
            .AddScoped<GetGraphUserService>()
            .AddScoped<SaveGraphUserService>();
    }
}

