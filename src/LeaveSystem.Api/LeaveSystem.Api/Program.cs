using System.Configuration;
using GoldenEye.Registration;
using LeaveSystem;
using LeaveSystem.Api.Auth;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.Db;
using LeaveSystem.Api.Endpoints.Employees;
using LeaveSystem.Api.Endpoints.Errors;
using LeaveSystem.Api.Endpoints.LeaveRequests;
using LeaveSystem.Api.Endpoints.Users;
using LeaveSystem.Api.Endpoints.WorkingHours;
using LeaveSystem.Api.GraphApi;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared.Converters;
using LeaveSystem.Shared.Date;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.UserLeaveLimits;
using Microsoft.AspNetCore.OData;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.IdentityModel.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

const string azureConfigSection = "AzureAdB2C";
const string azureReadUsersSection = "ManageAzureUsers";

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.Services.AddB2CAuthentication(builder.Configuration.GetSection(azureConfigSection));
builder.Services.AddRoleBasedAuthorization();
builder.Services.AddGraphFactory(builder.Configuration.GetSection(azureReadUsersSection));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddControllers()
    .AddJsonOptions(o =>  o.JsonSerializerOptions.Converters.Add(new TimeSpanIso8601Converter()))
    .AddOData(opt =>
    opt.AddRouteComponents("odata", GetEdmModel())
        .Select()
        .Filter()
        .Count()
        .Expand()
        .OrderBy());

IEdmModel GetEdmModel()
{
    var modelBuilder = new ODataConventionModelBuilder();
    modelBuilder.EntitySet<LeaveType>("LeaveTypes");
    modelBuilder.EntitySet<UserLeaveLimit>("UserLeaveLimits");
    modelBuilder.EntitySet<Setting>("Settings");
    modelBuilder.ComplexType<AddLeaveTypeDto>();

    return modelBuilder.GetEdmModel();
}

builder.Services.AddScoped<CurrentDateService>();
builder.Services.AddDDD();
builder.Services.AddLeaveSystemModule(builder.Configuration);
builder.Services.AddMvc();
builder.Services.AddValidators();
builder.Services.AddODataControllersServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(builder => builder
     .AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

    // Send "~/$odata" to debug routing if enable the following middleware
    app.UseODataRouteDebug();
}

app.UseMiddleware<ErrorHandlerMiddleware>(app.Environment.IsDevelopment(), app.Logger);

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

var azureScpes = app.Configuration[$"{azureConfigSection}:Scopes"];

app
    .AddLeaveRequestEndpoints(azureScpes)
    .AddWorkingHoursEndpoints(azureScpes)
    .AddEmployeesEndpoints(azureScpes)
    .AddUsersEndpoints(azureScpes);

await app.MigrateDb();
if (app.Environment.IsDevelopment())
{
    _ = app.FillInDatabase();
}

IdentityModelEventSource.ShowPII = true;

await app.RunAsync();
