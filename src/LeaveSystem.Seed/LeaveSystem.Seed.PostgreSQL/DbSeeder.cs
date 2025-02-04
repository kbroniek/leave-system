namespace LeaveSystem.Seed.PostgreSQL;

using LeaveSystem.Seed.PostgreSQL.Model;
using Microsoft.Azure.Cosmos;

internal class DbSeeder(OmbContext context, CosmosClient client)
{
    internal Task SeedUsers()
    {
        return Task.CompletedTask;
    }
}
