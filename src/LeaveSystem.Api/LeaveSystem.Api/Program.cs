using LeaveSystem;
using LeaveSystem.Api;
using LeaveSystem.Api.Auth;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.Endpoints.Employees;
using LeaveSystem.Api.Endpoints.Errors;
using LeaveSystem.Api.Endpoints.LeaveRequests;
using LeaveSystem.Api.Endpoints.Users;
using LeaveSystem.Api.Endpoints.WorkingHours;
using LeaveSystem.GraphApi;
using LeaveSystem.Shared.Converters;
using LeaveSystem.Shared.Date;
using Microsoft.AspNetCore.OData;

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
builder.Services.AddSwagger();
builder.Services.AddCors();
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new TimeSpanIso8601Converter()))
    .AddODataConfig();

builder.Services.AddServices(builder.Configuration, azureReadUsersSection)
    .AddScoped<DateService>()
    .AddValidators()
    .AddODataControllersServices();
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

//app.UseEndpoints(endpoints => endpoints.MapControllers());

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

var azureScpes = app.Configuration[$"{azureConfigSection}:Scopes"] ?? throw new InvalidOperationException($"{azureConfigSection}:Scopes configuration is missing. Check the appsettings.json.");

app
    .AddLeaveRequestEndpoints(azureScpes)
    .AddWorkingHoursEndpoints(azureScpes)
    .AddEmployeesEndpoints(azureScpes)
    .AddUsersEndpoints(azureScpes);

await app.RunAsync();
