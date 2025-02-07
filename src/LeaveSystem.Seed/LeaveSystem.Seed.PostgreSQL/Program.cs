using LeaveSystem.Seed.PostgreSQL;
using LeaveSystem.Seed.PostgreSQL.Model;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

Console.WriteLine("Configuring...");

var services = ConfigureServices();

var jsonSeeder = services.GetRequiredService<JsonSeeder>();
//await jsonSeeder.Seed("./Assets/LeaveStatusSettings.json");
//await jsonSeeder.Seed("./Assets/LeaveTypes.json");
//await jsonSeeder.Seed("./Assets/LeaveLimits.json"); // Empty
//await jsonSeeder.Seed("./Assets/LeaveRequests.json"); // Empty
//await jsonSeeder.Seed("./Assets/Roles.json"); // Empty

var graphSeeder = services.GetRequiredService<GraphSeeder>();
var users = await graphSeeder.SeedUsers();

var dbSeeder = services.GetRequiredService<DbSeeder>();
//await dbSeeder.SeedRoles(users);
//await dbSeeder.SeedLimits(users);
await dbSeeder.SeedLeaveRequests(users);

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
        .AddSingleton<DbSeeder>()
        .AddSingleton(p => new GraphSeeder(p.GetRequiredService<OmbContext>(), p.GetRequiredService<GraphServiceClient>(), defaultUsersPassword, b2cIssuer))
        .AddSingleton(_ => new LeaveSystem.Seed.PostgreSQL.GraphClientFactory(graphSettings.TenantId, graphSettings.ClientId, graphSettings.Secret, graphSettings.Scopes).Create());

    return services.BuildServiceProvider();
}

public static class ConsoleExtensions
{
    public static void WriteError(this string message) => message.WriteLine(ConsoleColor.Red);
    public static void WriteError(this object value) => value.WriteLine(ConsoleColor.Red);
    public static void WriteWarning(this string message) => message.WriteLine(ConsoleColor.Yellow);
    public static void WriteLine(this string message, ConsoleColor color)
    {
        var backup = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = backup;
    }
    public static void WriteLine(this object value, ConsoleColor color)
    {
        var backup = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(value);
        Console.ForegroundColor = backup;
    }
}

public static class FeedInteratorExtensions
{
    internal static async Task<(IReadOnlyList<T> results, string? continuationToken)> ToListAsync<T>(this FeedIterator<T> iterator, int pageSize = 1000, CancellationToken cancellationToken = default)
    {
        using (iterator)
        {
            List<T> results = [];
            while (iterator.HasMoreResults)
            {
                var queryResult = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(queryResult);
                if (results.Count >= pageSize)
                {
                    return (results, queryResult.ContinuationToken);
                }
            }

            return (results, null);
        }
    }
    internal static async Task<T?> FirstOrDefaultAsync<T>(this FeedIterator<T> iterator, CancellationToken cancellationToken = default)
    {
        using (iterator)
        {
            if (iterator.HasMoreResults)
            {
                var queryResult = await iterator.ReadNextAsync(cancellationToken);
                return queryResult.FirstOrDefault();
            }

            return default;
        }
    }
}

public class GraphSettings
{
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string Secret { get; set; }
    public string[] Scopes { get; set; }
}
