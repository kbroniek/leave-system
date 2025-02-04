using LeaveSystem.Seed.PostgreSQL;
using LeaveSystem.Seed.PostgreSQL.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Configuring...");

var services = ConfigureServices();

var jsonSeeder = services.GetRequiredService<JsonSeeder>();
await jsonSeeder.Seed("./Assets/LeaveStatusSettings.json");
await jsonSeeder.Seed("./Assets/LeaveTypes.json");

var dbSeeder = services.GetRequiredService<DbSeeder>();
await dbSeeder.SeedUsers();

static IServiceProvider ConfigureServices()
{
    IServiceCollection services = new ServiceCollection();

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddUserSecrets("1489f55f-2b46-454c-a22d-d58a5d5ff308")
        .Build();
    services.AddSingleton<IConfiguration>(configuration)
        .AddSingleton(_ => new CosmosClient(configuration.GetConnectionString("CosmosDBConnection")))
        .AddDbContext<OmbContext>(x => x.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
        .AddSingleton<JsonSeeder>()
        .AddSingleton<DbSeeder>();

    return services.BuildServiceProvider();
}
