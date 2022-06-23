using GoldenEye.Commands;
using GoldenEye.Registration;
using LeaveSystem;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.AddLeaveRequest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
builder.Services.AddAuthorization(options =>
      options.AddPolicy("Something",
      policy => policy.RequireClaim("extension_Role", "Administrator")));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddControllers().AddOData(opt =>
    opt.AddRouteComponents("odata", GetEdmModel())
        .Select()
        .Filter()
        .Count()
        .Expand()
        .OrderBy());

IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.Namespace = "LeaveSystem";
    builder.ContainerName = "LeaveSystemContainer";
    builder.EntitySet<LeaveType>("LeaveTypes");
    builder.EntitySet<Department>("Departments");
    builder.EntitySet<UserLeaveLimit>("UserLeaveLimits");
    builder.EntitySet<UserLeaveLimit>("Roles");

    return builder.GetEdmModel();
}

builder.Services.AddDDD();
builder.Services.AddLeaveSystemModule(builder.Configuration);

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

var scopeRequiredByApi = app.Configuration["AzureAdB2C:Scopes"];

//app.MapLeaveTypeEndpoints(scopeRequiredByApi);

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", (HttpContext httpContext) =>
//{
//    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);

//    var forecast = Enumerable.Range(1, 5).Select(index =>
//       new WeatherForecast
//       (
//           DateTime.Now.AddDays(index),
//           Random.Shared.Next(-20, 55),
//           summaries[Random.Shared.Next(summaries.Length)]
//       ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast")
//.RequireAuthorization("Something");

app.MapPost("/leaverequest", (HttpContext httpContext, ICommandBus commandBus, LeaveRequestModel leaveRequest) =>
{
    httpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
    var command = CreateLeaveRequest.Create(
                Guid.NewGuid(),
                leaveRequest.DateFrom,
                leaveRequest.DateTo,
                leaveRequest.Hours,
                leaveRequest.Type,
                leaveRequest.Remarks
            );
    return commandBus.Send(command);

})
.WithName("AddLeaveRequest")
.RequireAuthorization();

app.Run();