using LeaveSystem.Seed.PostgreSQL;
using LeaveSystem.Seed.PostgreSQL.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

Console.WriteLine("Configuring...");

var services = ConfigureServices();

//var jsonSeeder = services.GetRequiredService<JsonSeeder>();
//await jsonSeeder.Seed("./Assets/LeaveStatusSettings.json");
//await jsonSeeder.Seed("./Assets/LeaveTypes.json");

var graphSeeder = services.GetRequiredService<GraphSeeder>();
await graphSeeder.SeedUsers();

static IServiceProvider ConfigureServices()
{
    IServiceCollection services = new ServiceCollection();

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddUserSecrets("1489f55f-2b46-454c-a22d-d58a5d5ff308")
        .Build();
    var graphSettings = configuration.GetSection("WriteAzureUsers").Get<GraphSettings>();
    var defaultUsersPassword = configuration.GetValue<string>("DefaultUsersPassword");
    var b2cIssuer = configuration.GetValue<string>("B2CIssuer");
    services.AddSingleton<IConfiguration>(configuration)
        .AddSingleton(_ => new CosmosClient(configuration.GetConnectionString("CosmosDBConnection")))
        .AddDbContext<OmbContext>(x => x.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnectionString")))
        .AddSingleton<JsonSeeder>()
        .AddSingleton(p => new GraphSeeder(p.GetRequiredService<OmbContext>(), p.GetRequiredService<GraphServiceClient>(), defaultUsersPassword, b2cIssuer))
        .AddSingleton(_ => new LeaveSystem.Seed.PostgreSQL.GraphClientFactory(graphSettings.TenantId, graphSettings.ClientId, graphSettings.Secret, graphSettings.Scopes).Create());

    return services.BuildServiceProvider();
}


public class GraphSettings
{
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string Secret { get; set; }
    public string[] Scopes { get; set; }
}
