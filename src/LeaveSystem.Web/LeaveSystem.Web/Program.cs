using Blazored.Toast;
using LeaveSystem.Web;
using LeaveSystem.Web.Shared;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRadzenComponents();
builder.Services.AddBlazoredToast();
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddLeaveSystemModule();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<UniversalHttpService>();

await builder.Build().RunAsync();
