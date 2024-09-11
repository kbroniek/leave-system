
using System.Collections;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

var assets = Directory.GetFiles("Assets", "*.json");
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets("59d4d40d-b685-4560-a190-90b91aa17863")
    .Build();
var client = new CosmosClient(configuration.GetConnectionString("CosmosDBConnection"));
Database database = await client.CreateDatabaseIfNotExistsAsync("LeaveSystem");

foreach (var asset in assets)
{
    var fileName = Path.GetFileNameWithoutExtension(asset);
    Console.WriteLine($"Write data to {fileName}");

    var stream = await File.ReadAllTextAsync(asset);
    var converter = new ExpandoObjectConverter();
    dynamic contents = JsonConvert.DeserializeObject(stream, new JsonSerializerSettings { Converters = { converter } });
    var containerBuilder = database.DefineContainer(name: fileName, partitionKeyPath: contents.partitionKey.ToString());
    if (contents.uniqueKeys is IEnumerable uniqueKeysCollection)
    {
        foreach (JValue uniqueKeysCombined in uniqueKeysCollection)
        {
            var uniqueKeysSplit = uniqueKeysCombined.ToString().Split(',');
            var uniqueKeyDefinition = containerBuilder.WithUniqueKey();
            foreach (var uniqueKey in uniqueKeysSplit)
            {
                uniqueKeyDefinition.Path(uniqueKey);
            }
            uniqueKeyDefinition.Attach();
        }
    }
    Container container = await containerBuilder.CreateIfNotExistsAsync();
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
