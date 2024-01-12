using LeaveSystem.Shared.Auth;
using LeaveSystem.Web.Pages.HrPanel;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequestDetails;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.UserPanel;
using LeaveSystem.Web.Pages.UsersManagement;
using LeaveSystem.Web.Pages.WorkingHours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace LeaveSystem.Web;

public static class Config
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        const string azureConfig = "AzureAdB2C";
        var scopes = configuration.GetValue<string>($"{azureConfig}:Scopes")
            ?? throw new InvalidOperationException($"Can't find configuration {azureConfig}:Scopes");

        services
            .AddMsalAuthentication(configuration, azureConfig, scopes)
            .AddHttpClient(configuration, scopes);
    }
    public static void AddAuthorization(this IServiceCollection services) =>
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
            options.AddPolicy(UsersPage.PolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.UserAdmin)));
            options.AddPolicy(LeaveRequestDetails.AcceptPolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker)));
        }).AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
    public static IServiceCollection AddLeaveSystemModule(this IServiceCollection services) =>
        services
            .AddTransient<TimelineComponent>()
            .AddTransient<LeaveTypesService>()
            .AddTransient<GetLeaveRequestsService>()
            .AddTransient<UserLeaveLimitsService>()
            .AddTransient<EmployeeService>()
            .AddTransient<WorkingHoursService>()
            .AddTransient<HrSummaryService>()
            .AddTransient<GetLeaveStatusSettingsService>()
            .AddTransient<UsersService>()
            .AddTransient<ChangeStatusService>();
    private static IServiceCollection AddHttpClient(this IServiceCollection services, IConfiguration configuration, string scopes)
    {
        const string apiName = "LeaveSystem.Api";
        var apiAddress = configuration.GetValue<string>(apiName)
            ?? throw new InvalidOperationException($"Can't find configuration api adress {apiName}");
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
