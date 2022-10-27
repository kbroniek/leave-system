using LeaveSystem.Api.Endpoints;
using LeaveSystem.Db.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace LeaveSystem.Api.Auth;

public static class Config
{
    public static void AddB2CAuthentication(this IServiceCollection services, IConfigurationSection configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration);
    }
    public static void AddRoleBasedAuthorization(this IServiceCollection services)
    {
        services
            .AddLeaveRequestsAuthorization()
            .AddWorkingHoursAuthorization();
        services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
    }

    private static IServiceCollection AddLeaveRequestsAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestEndpoints.GetLeaveRequestsPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestEndpoints.CreateLeaveRequestPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestEndpoints.AcceptLeaveRequestPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestEndpoints.RejectLeaveRequestPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestEndpoints.CancelLeaveRequestPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))));
        return services;
    }

    private static IServiceCollection AddWorkingHoursAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorization(options =>
                options.AddPolicy(WorkingHoursEndpoints.GetWorkingHoursEndpointsPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))))
            .AddAuthorization(options =>
                options.AddPolicy(WorkingHoursEndpoints.GetUserWorkingHoursEndpointsPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))))
            .AddAuthorization(options =>
                options.AddPolicy(WorkingHoursEndpoints.GetUserWorkingHoursDurationEndpointsPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))));
        return services;
    }
}

