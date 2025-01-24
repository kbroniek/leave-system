namespace LeaveSystem.Seed.PostgreSQL;
using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

internal class JsonSeeder(CosmosClient client)
{
    internal async Task Seed(string path)
    {
        Database database = await client.CreateDatabaseIfNotExistsAsync("LeaveSystem");
        var databaseName = Path.GetFileNameWithoutExtension(path);
        Console.WriteLine($"Writing data to {databaseName}");

        var stream = await File.ReadAllTextAsync(path);
        var converter = new ExpandoObjectConverter();
        dynamic contents = JsonConvert.DeserializeObject(stream, new JsonSerializerSettings { Converters = { converter } });
        var containerBuilder = database.DefineContainer(name: databaseName, partitionKeyPath: contents.partitionKey.ToString());
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
            Console.WriteLine($"The file {path} is not enumerable.");
            return;
        }
        var i = 0;
        foreach (var item in items)
        {
            await container.UpsertItemAsync(item);
            ++i;
        }
        Console.WriteLine($"Saved {i} items to {databaseName}");
    }
}
