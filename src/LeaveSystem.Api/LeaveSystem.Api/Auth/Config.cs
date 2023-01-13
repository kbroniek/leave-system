using LeaveSystem.Api.Endpoints.Employees;
using LeaveSystem.Api.Endpoints.LeaveRequests;
using LeaveSystem.Api.Endpoints.Roles;
using LeaveSystem.Api.Endpoints.WorkingHours;
using LeaveSystem.Shared.Auth;
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
            .AddWorkingHoursAuthorization()
            .AddEmployeesAuthorization()
            .AddRolesAuthorization();
        services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
    }

    private static IServiceCollection AddLeaveRequestsAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestsEndpoints.GetLeaveRequestsPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestsEndpoints.LeaveRequestDetailsPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestsEndpoints.CreateLeaveRequestPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.Employee))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestsEndpoints.CreateLeaveRequestOnBehalfPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestsEndpoints.AcceptLeaveRequestPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestsEndpoints.RejectLeaveRequestPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker))))
            .AddAuthorization(options =>
                options.AddPolicy(LeaveRequestsEndpoints.CancelLeaveRequestPolicyName,
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
    private static IServiceCollection AddEmployeesAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorization(options =>
                options.AddPolicy(EmployeesEndpoints.GetEmployeesPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.DecisionMaker))));
        return services;
    }
    private static IServiceCollection AddRolesAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorization(options =>
                options.AddPolicy(RolesEndpoints.GetRolesPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.UserAdmin))));
        services
            .AddAuthorization(options =>
                options.AddPolicy(RolesEndpoints.UpdateRolesPolicyName,
                policy => policy.Requirements.Add(new RoleRequirement(RoleType.UserAdmin))));
        return services;
    }
}

