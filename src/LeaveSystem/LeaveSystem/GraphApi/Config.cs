using LeaveSystem.Domain.Employees;
using LeaveSystem.Domain.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveSystem.GraphApi;

public static class Config
{
    public class AppSettings
    {
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? Secret { get; set; }
        public string[]? Scopes { get; set; }
        public string? B2cExtensionAppClientId { get; set; }
        public string? DefaultPassword { get; set; }
        public string? Issuer { get; set; }
    }

    public static IServiceCollection AddGraphFactory(this IServiceCollection services, IConfigurationSection configuration)
    {
        var settings = configuration.Get<AppSettings>() ?? throw new InvalidOperationException("Azure AppSettings configuration is missing. Check the appsettings.json.");
        return services
            .AddScoped<IGraphClientFactory, GraphClientFactory>(_ => GraphClientFactory.Create(
                settings.TenantId,
                settings.ClientId,
                settings.Secret,
                settings.Scopes))
            .AddScoped(_ => RoleAttributeNameResolver.Create(settings.B2cExtensionAppClientId))
            .AddScoped<GetGraphUserService>()
            .AddScoped(sp => SaveGraphUserService.Create(
                sp.GetService<IGraphClientFactory>(),
                sp.GetService<RoleAttributeNameResolver>(),
                settings.DefaultPassword,
                settings.Issuer));
    }
}

