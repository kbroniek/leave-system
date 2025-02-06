namespace LeaveSystem.Seed.PostgreSQL;
using System;
using System.Data;
using System.Threading.Tasks;
using LeaveSystem.Seed.PostgreSQL.Model;
using LeaveSystem.Shared.Auth;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;

internal class DbSeeder(CosmosClient client, OmbContext ombContext)
{
    private const string DatabaseName = "LeaveSystem";
    private const int DefaultWorkingHours = 8;

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

    internal async Task SeedLimits(IReadOnlyCollection<CreatedUser> users)
    {
        Console.WriteLine("Getting limits from DB");
        var limits = await ombContext.Userleavelimits
            .ToListAsync();
        Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseName);
        var container = database.GetContainer("LeaveLimits");
        var containerLeaveTypes = database.GetContainer("LeaveTypes");
        Console.WriteLine("Inserting limits to CosmosDB");
        var infiniteLimitsInsertedCount = await InsertInfiniteLimits(limits, container, containerLeaveTypes);
        var limitsForUsersInsertedCount = await InsertLimitsForUsers(limits, container, containerLeaveTypes, users);
        Console.WriteLine($"\rSaved {infiniteLimitsInsertedCount + limitsForUsersInsertedCount} items to the Roles table");
    }

    internal async Task SeedLeaveRequests(IReadOnlyCollection<CreatedUser> users)
    {

    }

    private static async Task<int> InsertLimitsForUsers(IReadOnlyCollection<Userleavelimit> limits, Container container, Container containerLeaveTypes, IReadOnlyCollection<CreatedUser> users)
    {
        var i = 0;
        foreach (var limit in limits
            .Where(x => x.Validsince != null))
        {
            var leaveTypeFromDB = await containerLeaveTypes.GetItemLinqQueryable<LeaveTypesEntity>()
                .Where(x => x.oldId == limit.LeavetypeLeavetypeid)
                .ToFeedIterator()
                .FirstOrDefaultAsync();

            if (leaveTypeFromDB == null)
            {
                $"Can't find leave type (id: {limit.LeavetypeLeavetypeid}) in the cosmosDB".WriteWarning();
                continue;
            }
            var user = users.FirstOrDefault(x => x.OldId == limit.UserUserid);
            if (user == null)
            {
                $"Can't find user (id: {limit.UserUserid}) in the graph users".WriteWarning();
                continue;
            }
            var limitFromDb = await container.GetItemLinqQueryable<LimitEntity>()
                .Where(x => x.oldId == limit.Userleavelimitid)
                .ToFeedIterator()
                .FirstOrDefaultAsync();

            await container.UpsertItemAsync(new
            {
                id = limitFromDb?.id ?? Guid.NewGuid(),
                limit = ConvertTime(limit.UserLimit),
                overdueLimit = ConvertTime(limit.Overduelimit),
                workingHours = TimeSpan.FromHours(DefaultWorkingHours).ToString(),
                leaveTypeId = leaveTypeFromDB?.id,
                validSince = limit.Validsince,
                validUntil = limit.Validuntil,
                assignedToUserId = user.Id,
                oldId = limit.Userleavelimitid,
                description = limit.Description,
            });
            ++i;
            Console.Write($"\rInserted {i} items");
        }

        return i;

    }

    private static async Task<int> InsertInfiniteLimits(IReadOnlyCollection<Userleavelimit> limits, Container container, Container containerLeaveTypes)
    {
        var i = 0;
        foreach (var limitGroupped in limits
            .Where(x => x.Validsince == null)
            .GroupBy(x => x.LeavetypeLeavetypeid))
        {
            var limit = limitGroupped.First();
            var leaveTypeFromDB = await containerLeaveTypes.GetItemLinqQueryable<LeaveTypesEntity>()
                .Where(x => x.oldId == limit.LeavetypeLeavetypeid)
                .ToFeedIterator()
                .FirstOrDefaultAsync();

            if (leaveTypeFromDB == null)
            {
                $"Can't find leave type (id: {limit.LeavetypeLeavetypeid}) in the cosmosDB".WriteWarning();
                continue;
            }
            var limitFromDb = await container.GetItemLinqQueryable<LimitEntity>()
                .Where(x => x.oldId == limitGroupped.Key)
                .ToFeedIterator()
                .FirstOrDefaultAsync();

            await container.UpsertItemAsync(new
            {
                id = limitFromDb?.id ?? Guid.NewGuid(),
                limit = ConvertTime(limit.UserLimit),
                leaveTypeId = leaveTypeFromDB?.id,
                oldId = limitGroupped.Key //Intentionally set leave type instead of leave limit
            });
            ++i;
            Console.Write($"\rInserted {i} items");
        }

        return i;
    }

    private static string? ConvertTime(double? limit) => limit is null or 0 ? null : TimeSpan.FromHours(limit.Value * DefaultWorkingHours).ToString();

    private sealed record LeaveTypesEntity(Guid id, int oldId);
    private sealed record LimitEntity(Guid id, int oldId);

    private static string? MapRoleName(string role) => role switch
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
