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
    private const string CreatedLeaveRequestEventType = "LeaveSystem.Domain.LeaveRequests.Creating.LeaveRequestCreated, LeaveSystem.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    private const string CanceledLeaveRequestEventType = "LeaveSystem.Domain.LeaveRequests.Canceling.LeaveRequestCanceled, LeaveSystem.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    private const string RejectedLeaveRequestEventType = "LeaveSystem.Domain.LeaveRequests.Rejecting.LeaveRequestRejected, LeaveSystem.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    private const string AcceptedLeaveRequestEventType = "LeaveSystem.Domain.LeaveRequests.Accepting.LeaveRequestAccepted, LeaveSystem.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    private const string DatabaseName = "LeaveSystem";
    private const int DefaultWorkingHours = 8;
    private static readonly string DefaultWorkingHoursTxt = TimeSpan.FromHours(DefaultWorkingHours).ToString();
    private static readonly TimeOnly noon = new TimeOnly(12, 00, 00);

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
        Console.WriteLine($"\rSaved {infiniteLimitsInsertedCount + limitsForUsersInsertedCount} items to the LeaveLimits table");
    }

    internal async Task SeedLeaveRequests(IReadOnlyCollection<CreatedUser> users)
    {
        Console.WriteLine("Getting leave requests from DB");
        var leaveRequests = await ombContext.Leaverequests
            .ToListAsync();
        Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseName);
        var container = database.GetContainer("LeaveRequests");
        var containerLeaveTypes = database.GetContainer("LeaveTypes");
        var i = 0;
        foreach (var leaveRequest in leaveRequests)
        {
            var leaveTypeFromDB = await containerLeaveTypes.GetItemLinqQueryable<LeaveTypesEntity>()
                .Where(x => x.oldId == leaveRequest.LeavetypeLeavetypeid)
                .ToFeedIterator()
                .FirstOrDefaultAsync();

            if (leaveTypeFromDB == null)
            {
                $"Can't find leave type (id: {leaveRequest.LeavetypeLeavetypeid}) in the cosmosDB".WriteWarning();
                continue;
            }
            var user = users.FirstOrDefault(x => x.OldId == leaveRequest.UserUserid);
            if (user == null)
            {
                $"Can't find user (id: {leaveRequest.UserUserid}) in the graph users".WriteWarning();
                continue;
            }
            var (leaveRequestsFromDb, _) = await container.GetItemLinqQueryable<LeaveRequestEntity>()
                .Where(x => x.oldId == leaveRequest.Leaverequestid)
                .ToFeedIterator()
                .ToListAsync();

            var createdLeaveRequest = leaveRequestsFromDb.FirstOrDefault(x => x.eventType == CreatedLeaveRequestEventType);
            var streamId = createdLeaveRequest?.streamId ?? Guid.NewGuid();
            await container.UpsertItemAsync(new
            {
                id = createdLeaveRequest?.id ?? Guid.NewGuid(),
                streamId,
                body = new
                {
                    leaveRequestId = streamId,
                    dateFrom = leaveRequest.Startday,
                    dateTo = leaveRequest.Endday,
                    duration = ConvertHoursToTime(leaveRequest.Hours),
                    leaveTypeId = leaveTypeFromDB?.id,
                    remarks = leaveRequest.Description,
                    createdBy = new
                    {
                        id = user.Id,
                        name = user.Name
                    },
                    assignedTo = new
                    {
                        id = user.Id,
                        name = user.Name
                    },
                    workingHours = DefaultWorkingHoursTxt,
                    createdDate = new DateTimeOffset(leaveRequest.Submissiondate?.ToDateTime(noon) ?? leaveRequest.Modificationdate ?? leaveRequest.Startday.ToDateTime(noon)),
                    streamId
                },
                oldId = leaveRequest.Leaverequestid,
                eventType = CreatedLeaveRequestEventType
            });
            if (leaveRequest.StatusLeaverequeststatusid is not 6 and not 1) // Not Pending
            {
                var eventType = MapToEventType(leaveRequest.StatusLeaverequeststatusid);
                if (eventType == null)
                {
                    $"Can't map leave request status (id: {leaveRequest.StatusLeaverequeststatusid}) to event type".WriteWarning();
                    continue;
                }
                var decideUser = users.FirstOrDefault(x => x.OldId == leaveRequest.DecideuserUserid) ?? user;
                var foundLeaveRequest = leaveRequestsFromDb.FirstOrDefault(x => x.eventType == eventType);

                await container.UpsertItemAsync(new
                {
                    id = foundLeaveRequest?.id ?? Guid.NewGuid(),
                    streamId,
                    body = new
                    {
                        leaveRequestId = streamId,
                        remarks = leaveRequest.Decisiondescription,
                        acceptedBy = new
                        {
                            id = decideUser.Id,
                            name = decideUser.Name
                        },
                        createdDate = new DateTimeOffset(leaveRequest.Modificationdate ?? leaveRequest.Submissiondate?.ToDateTime(noon) ?? leaveRequest.Startday.ToDateTime(noon)),
                        streamId
                    },
                    oldId = leaveRequest.Leaverequestid,
                    eventType
                });
            }

            ++i;
            Console.Write($"\rInserted {i} items");
        }
        Console.WriteLine($"\rSaved {i} items to the LeaveRequests table");
    }

    private static string? MapToEventType(int status) => status switch
    {
        2 => RejectedLeaveRequestEventType,
        3 => AcceptedLeaveRequestEventType,
        4 => CanceledLeaveRequestEventType,
        5 => AcceptedLeaveRequestEventType,
        _ => null
    };

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
                limit = ConvertLimitToTime(limit.UserLimit),
                overdueLimit = ConvertLimitToTime(limit.Overduelimit),
                workingHours = DefaultWorkingHoursTxt,
                leaveTypeId = leaveTypeFromDB?.id,
                validSince = limit.Validsince,
                validUntil = limit.Validuntil,
                assignedToUserId = user.Id,
                description = limit.Description,
                state = "Active",
                oldId = limit.Userleavelimitid,
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
                limit = ConvertLimitToTime(limit.UserLimit),
                leaveTypeId = leaveTypeFromDB?.id,
                state = "Active",
                oldId = limitGroupped.Key //Intentionally set leave type instead of leave limit
            });
            ++i;
            Console.Write($"\rInserted {i} items");
        }

        return i;
    }

    private static string? ConvertLimitToTime(double? limit) => limit is null or 0 ? null : TimeSpan.FromHours(limit.Value * DefaultWorkingHours).ToString();
    private static string? ConvertHoursToTime(double? hours) => hours is null or 0 ? null : TimeSpan.FromHours(hours.Value).ToString();

    private sealed record LeaveTypesEntity(Guid id, int oldId);
    private sealed record LimitEntity(Guid id, int oldId);
    private sealed record LeaveRequestEntity(Guid id, Guid streamId, string eventType, int oldId);

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
