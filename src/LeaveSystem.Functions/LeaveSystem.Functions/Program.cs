using System.Diagnostics;
using System.Security.Claims;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using LeaveSystem.Functions.Shared;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseMiddleware<ExceptionHandlingMiddleware>();
        builder.UseFunctionsAuthorization();
    })
    .ConfigureServices((context, services) =>
    {
        if (!context.HostingEnvironment.IsDevelopment())
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();
            services.AddOpenTelemetry().UseAzureMonitor();
        }
        services
            .AddFunctionsAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtFunctionsBearer(options =>
            {
                options.Authority = "https://leavesystem.b2clogin.com/tfp/leavesystem.onmicrosoft.com/b2c_1a_signincustom_sspr/v2.0/";
                options.Audience = "4f24b978-403f-47fe-9cae-52deea03661d";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    // ...
                    OnMessageReceived = context =>
                    {
                        // Cannot use Authorization header because of https://github.com/Azure/static-web-apps/issues/34
                        string authorization = context.Request.Headers["X-Authorization"];

                        // If no authorization header found, nothing to process further
                        if (string.IsNullOrEmpty(authorization))
                        {
                            context.NoResult();
                            return Task.CompletedTask;
                        }

                        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authorization["Bearer ".Length..].Trim();
                        }

                        // If no token found, no further work possible
                        if (string.IsNullOrEmpty(context.Token))
                        {
                            context.NoResult();
                            return Task.CompletedTask;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
    })
    .Build();

host.Run();
