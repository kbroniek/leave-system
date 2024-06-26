namespace LeaveSystem.Api;

using GoldenEye.Backend.Core.DDD.Registration;
using LeaveSystem.Db.Entities;
using LeaveSystem.GraphApi;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;

public static class Config
{
    public static IServiceCollection AddSwagger(this IServiceCollection services) =>
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Leave System API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                    },
                    new List<string>()
                }
            });
        });
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration, string azureReadUsersSection) =>
        services.AddGraphFactory(configuration.GetSection(azureReadUsersSection))
            .AddDDD()
            .AddLeaveSystemModule(configuration);

    public static IMvcBuilder AddODataConfig(this IMvcBuilder mvcBuilder) =>
        mvcBuilder.AddOData(opt =>
            opt.AddRouteComponents("odata", GetEdmModel())
                .Select()
                .Filter()
                .Count()
                .Expand()
                .OrderBy());

    private static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.EntitySet<LeaveType>("LeaveTypes");
        modelBuilder.EntitySet<UserLeaveLimit>("UserLeaveLimits");
        modelBuilder.EntitySet<Setting>("Settings");
        return modelBuilder.GetEdmModel();
    }
}
