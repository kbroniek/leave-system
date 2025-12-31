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
    internal async Task<IEnumerable<dynamic>> Seed(string path)
    {
        Database database = await client.CreateDatabaseIfNotExistsAsync("LeaveSystem");
        var containerName = Path.GetFileNameWithoutExtension(path);
        Console.WriteLine($"Creating container {containerName}");

        var stream = await File.ReadAllTextAsync(path);
        var converter = new ExpandoObjectConverter();
        dynamic contents = JsonConvert.DeserializeObject(stream, new JsonSerializerSettings { Converters = { converter } });
        var containerBuilder = database.DefineContainer(name: containerName, partitionKeyPath: contents.partitionKey.ToString());
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
            throw new InvalidOperationException($"The file {path} is not enumerable.");
        }
        Console.WriteLine($"Writing data to {containerName}");
        var i = 0;
        foreach (var item in items)
        {
            await container.UpsertItemAsync(item);
            Console.Write($"\rInserted {i} items");
            ++i;
        }
        Console.WriteLine($"\rSaved {i} items to {containerName}");
        return contents.items;
    }
}
