namespace LeaveSystem.Api.Endpoints.Employees;

public static class Config
{
    public class AppSettings
    {
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? Secret { get; set; }
        public string[]? Scopes { get; set; }
    }

    public static void AddAzureReadUsers(this IServiceCollection services, IConfigurationSection configuration)
    {
        var settings = configuration.Get<AppSettings>();
        services
            .AddScoped(_ => GraphUserService.Create(settings.TenantId,
                                            settings.ClientId,
                                            settings.Secret,
                                            settings.Scopes));
    }
}

