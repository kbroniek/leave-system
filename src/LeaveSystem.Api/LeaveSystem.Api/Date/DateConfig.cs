using LeaveSystem.Shared.Date;

namespace LeaveSystem.Api.Date;

public static class DateConfig
{
    public static DateTimeOffset CustomBaseDate = DateTimeOffset.UtcNow;
    public static DateServiceType DateServiceType = DateServiceType.CurrentDateService;
    public static void AddBaseDateServices(this IServiceCollection services)
    {
        services.AddTransient<CustomDateService>(_ => new CustomDateService(CustomBaseDate))
            .AddTransient<CurrentDateService>()
            .AddTransient<IBaseDateService>(sp =>
            (DateServiceType switch
            {
                DateServiceType.CurrentDateService => sp.GetService<CurrentDateService>(),
                DateServiceType.CustomDateService => sp.GetService<CustomDateService>(),
                _ => sp.GetService<CurrentDateService>()
            })!);
    }
}