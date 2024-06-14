using System.Security.Claims;
using LeaveSystem.Functions.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseMiddleware<ExceptionHandlingMiddleware>();
        builder.UseFunctionsAuthorization();
    })

    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services
            .AddFunctionsAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtFunctionsBearer(options =>
            {
                options.Authority = "https://leavesystem.b2clogin.com/tfp/leavesystem.onmicrosoft.com/b2c_1a_signincustom_sspr/v2.0/";
                options.Audience = "114ea83d-c494-46f4-9d7c-b582fed7b5b9";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = ClaimTypes.Role
                };
            });
    })
    .Build();

host.Run();
