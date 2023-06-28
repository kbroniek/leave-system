using LeaveSystem.Shared.Auth;
using LeaveSystem.Web.Pages.HrPanel;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequestDetails;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.UserPanel;
using LeaveSystem.Web.Pages.WorkingHours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace LeaveSystem.Web;

public static class Config
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        const string AzureConfig = "AzureAdB2C";
        var scopes = configuration.GetValue<string>($"{AzureConfig}:Scopes");

        services
            .AddMsalAuthentication(configuration, AzureConfig, scopes)
            .AddHttpClient(configuration, scopes);
    }
    public static void AddAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(CreateLeaveRequest.OnBehalfPolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker)));
            options.AddPolicy(CreateLeaveRequest.PolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.Employee, RoleType.DecisionMaker)));
            options.AddPolicy(LeaveRequestDetails.PolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.Employee, RoleType.DecisionMaker)));
            options.AddPolicy(ShowLeaveRequests.PolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.Employee, RoleType.DecisionMaker)));
            options.AddPolicy(ShowUserPanel.PolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.Employee)));
            options.AddPolicy(ShowHrPanel.PolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.HumanResource)));
        });
        services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
    }
    public static IServiceCollection AddLeaveSystemModule(this IServiceCollection services)
    {
        return services
            .AddTransient<TimelineComponent>()
            .AddTransient<StyleGenerator>()
            .AddTransient<LeaveTypesService>()
            .AddTransient<GetLeaveRequestsService>()
            .AddTransient<UserLeaveLimitsService>()
            .AddTransient<EmployeeService>()
            .AddTransient<WorkingHoursService>()
            .AddTransient<LeaveRequestSummaryService>()
            .AddTransient<GetLeaveStatusSettingsService>();
    }
    private static IServiceCollection AddHttpClient(this IServiceCollection services, IConfiguration configuration, string scopes)
    {
        const string apiName = "LeaveSystem.Api";
        var apiAddress = configuration.GetValue<string>(apiName);
        services.AddHttpClient(apiName, client => client.BaseAddress = new Uri(apiAddress))
            .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
            .ConfigureHandler(
                authorizedUrls: new[] { apiAddress },
                scopes: new[] { scopes }));
        return services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(apiName));
    }

    private static IServiceCollection AddMsalAuthentication(this IServiceCollection services, IConfiguration configuration, string AzureConfig, string scopes)
    {
        services.AddMsalAuthentication(options =>
        {
            configuration.Bind(AzureConfig, options.ProviderOptions.Authentication);
            options.ProviderOptions.DefaultAccessTokenScopes.Add(scopes);
        });
        return services;
    }
}
