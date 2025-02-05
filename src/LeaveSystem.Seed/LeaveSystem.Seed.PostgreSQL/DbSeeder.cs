namespace LeaveSystem.Seed.PostgreSQL;
using System;
using System.Threading.Tasks;
using LeaveSystem.Seed.PostgreSQL.Model;
using LeaveSystem.Shared.Auth;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

internal class DbSeeder(CosmosClient client, OmbContext ombContext)
{
    private const string DatabaseName = "LeaveSystem";

    internal async Task SeedRoles(IReadOnlyCollection<CreatedUser> users)
    {
        Console.WriteLine("Getting roles from DB");
        var roles = await ombContext.Userroles
            .Select(x => new { x.Role, UserId = x.UserUserid })
            .ToListAsync();
        Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseName);
        var container = database.GetContainer("Roles");
        Console.WriteLine("Inserting roles to CosmosDB");
        var i = 0;
        foreach (var role in roles
            .Select(x => new { Role = MapRoleName(x.Role), x.UserId })
            .Where(x => x.Role != null)
            .GroupBy(x => x.UserId))
        {
            var user = users.FirstOrDefault(x => x.OldId == role.Key);
            if (user == null)
            {
                $"Can't find user (id: {role.Key}) in the graph users".WriteWarning();
                continue;
            }
            await container.UpsertItemAsync(new
            {
                id = user.Id,
                roles = role.Select(x => x.Role),
                email = user.Email,
            });
            ++i;
        }
        Console.WriteLine($"Saved {i} items to the Roles table");
    }

    private string? MapRoleName(string role) => role switch
    {
        "ROLE_USER_ADMIN" => RoleType.UserAdmin.ToString(),
        "ROLE_ADMIN" => RoleType.GlobalAdmin.ToString(),
        "ROLE_EMPLOYEE" => RoleType.Employee.ToString(),
        "ROLE_USER" => null,
        "ROLE_HUMAN_RESOURCE" => RoleType.HumanResource.ToString(),
        "ROLE_LEAVE_DECISION" => RoleType.DecisionMaker.ToString(),
        "ROLE_SHOW_LEAVE_TYPE" => null,
        "ROLE_LEAVE_LIMIT_ADMIN" => RoleType.LeaveLimitAdmin.ToString(),
        _ => null
    };
}
