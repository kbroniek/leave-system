﻿using LeaveSystem.Web.Components;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.WorkingHours;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Authorization;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;

namespace LeaveSystem.Web;

public static class Config
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        const string AzureConfig = "AzureAdB2C";
        var scopes = configuration.GetValue<string>($"{AzureConfig}:Scopes");
        services.AddAuthentication(configuration, AzureConfig, scopes)
            .AddHttpClient(configuration, scopes);
    }
    public static void AddAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(CreateLeaveRequest.CreateOnBehalfPolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker)));
            options.AddPolicy(CreateLeaveRequest.CreatePolicyName, policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.Employee)));
        });
        services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
    }
    public static IServiceCollection AddLeaveSystemModule(this IServiceCollection services)
    {
        return services
            .AddTransient(sp => new TimelineComponent(sp.GetService<IJSRuntime>()))
            .AddTransient<LeaveTypesService>()
            .AddTransient<GetLeaveRequestsService>()
            .AddTransient<UserLeaveLimitsService>()
            .AddTransient<WorkingHoursService>();
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
        // Supply HttpClient instances that include access tokens when making requests to the server project
        return services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(apiName));
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration, string AzureConfig, string scopes)
    {
        services.AddMsalAuthentication(options =>
        {
            configuration.Bind(AzureConfig, options.ProviderOptions.Authentication);
            options.ProviderOptions.DefaultAccessTokenScopes.Add(scopes);
        });
        return services;
    }
}
