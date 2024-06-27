using System.Diagnostics;
using System.Security.Claims;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using LeaveSystem.Functions.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

try
{
    var host = new HostBuilder()
        .ConfigureFunctionsWebApplication(builder =>
        {
            builder.UseMiddleware<ExceptionHandlingMiddleware>();
            builder.UseFunctionsAuthorization();
        })
        .ConfigureServices((context, services) =>
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;
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
                    options.Authority = context.Configuration.GetValue<string>("JwtBearerOptions_Authority") ?? throw new InvalidOperationException("Cannot find JwtBearerOptions_Authority in the configuration.");
                    options.Audience = context.Configuration.GetValue<string>("JwtBearerOptions_Audience") ?? throw new InvalidOperationException("Cannot find JwtBearerOptions_Audience in the configuration.");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = ClaimTypes.Role
                    };
                    var authorizationHeaderName = context.Configuration.GetValue<string>("JwtBearerOptions_AuthorizationHeaderName") ?? throw new InvalidOperationException("Cannot find authorizationHeaderName in the configuration.");
                    options.Events = new JwtBearerEvents
                    {
                        // ...
                        OnMessageReceived = context =>
                        {
                            // Cannot use Authorization header because of https://github.com/Azure/static-web-apps/issues/34
                            var authorization = context.Request.Headers[authorizationHeaderName];

                            // If no authorization header found, nothing to process further
                            if (string.IsNullOrEmpty(authorization))
                            {
                                context.NoResult();
                                return Task.CompletedTask;
                            }
                            var authorizationStr = authorization.ToString();
                            if (authorizationStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Token = authorizationStr["Bearer ".Length..].Trim();
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
}
catch (Exception ex)
{
    // For development purposes
    throw;
}
