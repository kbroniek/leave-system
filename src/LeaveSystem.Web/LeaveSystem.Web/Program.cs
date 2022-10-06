using LeaveSystem.Web;
using LeaveSystem.Web.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

const string ApiName = "LeaveSystem.Api";
const string AzureConfig = "AzureAdB2C";
var apiAddress = builder.Configuration.GetValue<string>(ApiName);
var scopes = builder.Configuration.GetValue<string>($"{AzureConfig}:Scopes");

builder.Services.AddHttpClient(ApiName, client => client.BaseAddress = new Uri(apiAddress))
    .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
    .ConfigureHandler(
        authorizedUrls: new[] { apiAddress },
        scopes: new[] { scopes }));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(ApiName));

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind(AzureConfig, options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(scopes);
});
builder.Services.AddTransient(sp => new TimelineComponent(sp.GetService<IJSRuntime>()));

await builder.Build().RunAsync();
