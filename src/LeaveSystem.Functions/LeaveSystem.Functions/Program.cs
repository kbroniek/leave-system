using LeaveSystem.Shared.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

IdentityModelEventSource.ShowPII = true;
var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseFunctionsAuthorization();
    })
    .ConfigureServices(services =>
    {
        services
            .AddFunctionsAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "https://leavesystem.b2clogin.com/tfp/leavesystem.onmicrosoft.com/b2c_1_signin/v2.0/";
                options.Audience = "4f24b978-403f-47fe-9cae-52deea03661d";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "extension_Role"
                };

            });

        services.AddFunctionsAuthorization(options =>
        {
            options.AddPolicy("OnlyAdmins", policy =>
                policy.Requirements.Add(new RoleRequirement(RoleType.GlobalAdmin)));
        });
        services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();

        // Add other services
    })
    .Build();

host.Run();
