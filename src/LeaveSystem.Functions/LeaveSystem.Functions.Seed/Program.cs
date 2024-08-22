
using System.Collections;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

var assets = Directory.GetFiles("Assets", "*.json");
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets("59d4d40d-b685-4560-a190-90b91aa17863")
    .Build();
CosmosClient client = new CosmosClient(configuration.GetConnectionString("CosmosDBConnection"));
Database database = await client.CreateDatabaseIfNotExistsAsync("LeaveSystem");

foreach (var asset in assets)
{
    var fileName = Path.GetFileNameWithoutExtension(asset);
    Console.WriteLine($"Write data to {fileName}");

    var stream = await File.ReadAllTextAsync(asset);
    var converter = new ExpandoObjectConverter();
    dynamic contents = JsonConvert.DeserializeObject(stream, new JsonSerializerSettings { Converters = { converter } });
    Container container = await database.CreateContainerIfNotExistsAsync(fileName, $"/{contents.partitionKey}");
    if (contents.items is not IEnumerable items)
    {
        Console.WriteLine($"The file {asset} is not enumerable.");
        continue;
    }
    var i = 0;
    foreach (var item in items)
    {
        await container.UpsertItemAsync(item);
        ++i;
    }
    Console.WriteLine($"Saved {i} items to {fileName}");
}
