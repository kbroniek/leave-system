using LeaveSystem.Api.Endpoints.Employees;
using LeaveSystem.Api.Endpoints.LeaveRequests;
using LeaveSystem.Api.Endpoints.Users;
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
            .AddUsersAuthorization();
        services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
    }

    private static IServiceCollection AddLeaveRequestsAuthorization(this IServiceCollection services) =>
        services
            .AddAuthorizationPolicy(LeaveRequestsEndpoints.GetLeaveRequestsPolicyName, RoleType.Employee, RoleType.DecisionMaker, RoleType.HumanResource)
            .AddAuthorizationPolicy(LeaveRequestsEndpoints.GetLeaveRequestDetailsPolicyName, RoleType.Employee, RoleType.DecisionMaker)
            .AddAuthorizationPolicy(LeaveRequestsEndpoints.CreateLeaveRequestPolicyName, RoleType.Employee)
            .AddAuthorizationPolicy(LeaveRequestsEndpoints.CreateLeaveRequestOnBehalfPolicyName, RoleType.DecisionMaker)
            .AddAuthorizationPolicy(LeaveRequestsEndpoints.AcceptLeaveRequestPolicyName, RoleType.DecisionMaker)
            .AddAuthorizationPolicy(LeaveRequestsEndpoints.RejectLeaveRequestPolicyName, RoleType.DecisionMaker)
            .AddAuthorizationPolicy(LeaveRequestsEndpoints.CancelLeaveRequestPolicyName, RoleType.Employee);

    private static IServiceCollection AddWorkingHoursAuthorization(this IServiceCollection services) =>
        services
            .AddAuthorizationPolicy(WorkingHoursEndpoints.GetWorkingHoursEndpointsPolicyName, RoleRequirement.AuthorizeAll)
            .AddAuthorizationPolicy(WorkingHoursEndpoints.GetUserWorkingHoursEndpointsPolicyName, RoleRequirement.AuthorizeAll)
            .AddAuthorizationPolicy(WorkingHoursEndpoints.CreateWorkingHoursPolicyName, RoleType.WorkingHoursAdmin)
            .AddAuthorizationPolicy(WorkingHoursEndpoints.ModifyUserWorkingHoursPolicyName, RoleType.WorkingHoursAdmin);
    private static IServiceCollection AddEmployeesAuthorization(this IServiceCollection services) =>
        services
            .AddAuthorizationPolicy(EmployeesEndpoints.GetEmployeesPolicyName, RoleType.DecisionMaker)
            .AddAuthorizationPolicy(EmployeesEndpoints.GetEmployeePolicyName, RoleType.HumanResource);
    private static IServiceCollection AddUsersAuthorization(this IServiceCollection services) =>
        services
            .AddAuthorizationPolicy(UsersEndpoints.GetUsersPolicyName, RoleType.UserAdmin)
            .AddAuthorizationPolicy(UsersEndpoints.AddUserPolicyName, RoleType.UserAdmin)
            .AddAuthorizationPolicy(UsersEndpoints.UpdateUserPolicyName, RoleType.UserAdmin);

    private static IServiceCollection AddAuthorizationPolicy(this IServiceCollection services, string policyName, params RoleType[] roles) =>
        services
            .AddAuthorizationPolicy(policyName, new RoleRequirement(roles));

    private static IServiceCollection AddAuthorizationPolicy(this IServiceCollection services, string policyName, RoleRequirement roleRequirement) =>
        services
            .AddAuthorization(options =>
                options.AddPolicy(policyName,
                policy => policy.Requirements.Add(roleRequirement)));
}

