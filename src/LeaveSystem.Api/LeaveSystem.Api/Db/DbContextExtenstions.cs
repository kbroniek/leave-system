using Ardalis.GuardClauses;
using FluentAssertions.Common;
using GoldenEye.Commands;
using GoldenEye.Queries;
using GoldenEye.Registration;
using LeaveSystem.Api.Endpoints.Employees;
using LeaveSystem.Api.Endpoints.Users;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Auth;
using LeaveSystem.Shared.Date;
using LeaveSystem.Shared.LeaveRequests;
using Marten.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours;

namespace LeaveSystem.Api.Db;

public static class DbContextExtenstions
{
    private static readonly TimeSpan workingHours = TimeSpan.FromHours(8);
    private const string DefaultUserEmail = "karolbr5@gmail.com";

    private static readonly FederatedUser defaultUserMock = FederatedUser.Create("1c353785-c700-4a5d-bec5-a0f6f668bf22",
        DefaultUserEmail, "Karol Volt", new[] { RoleType.GlobalAdmin.ToString() });

    private static FederatedUser defaultUser;
    private static FederatedUser[] testUsers = Array.Empty<FederatedUser>();

    private static readonly FederatedUser[] testUserMock = new[]
    {
        FederatedUser.Create("aa379a52-7e8f-4471-b948-fbba4284bebb", "jkowalski@test.com", "Jan Kowalski",
            new[] { RoleType.DecisionMaker.ToString() }),
        FederatedUser.Create("88fa3c20-0c52-4da4-8389-868d9a487aa0", "mnowak@test.com", "Maria Nowak",
            new[] { RoleType.HumanResource.ToString() }),
        FederatedUser.Create("1374e2d6-15f5-4543-b7bf-95118701f315", "jszczepanek@test.com", "Jadwiga Szczepanek",
            new[] { RoleType.LeaveLimitAdmin.ToString() }),
        FederatedUser.Create("59ed14ff-edc4-421c-8f22-28973f4ccd76", "aszewczyk@test.com", "Aleksandra Szewczyk",
            new[] { RoleType.UserAdmin.ToString() }),
        FederatedUser.Create("d5ff6b57-a701-4ce8-82ef-593ef207fb76", "ourbanek@test.com", "Olgierd Urbanek",
            new[] { RoleType.Employee.ToString() })
    };

    private static Setting[] settings = new Setting[]
    {
        new Setting
        {
            Id = LeaveRequestStatus.Canceled.ToString(), Category = SettingCategoryType.LeaveStatus,
            Value = JsonDocument.Parse("{\"color\": \"#525252\"}")
        },
        new Setting
        {
            Id = LeaveRequestStatus.Rejected.ToString(), Category = SettingCategoryType.LeaveStatus,
            Value = JsonDocument.Parse("{\"color\": \"#850000\"}")
        },
        new Setting
        {
            Id = LeaveRequestStatus.Pending.ToString(), Category = SettingCategoryType.LeaveStatus,
            Value = JsonDocument.Parse("{\"color\": \"#CFFF98\"}")
        },
        new Setting
        {
            Id = LeaveRequestStatus.Accepted.ToString(), Category = SettingCategoryType.LeaveStatus,
            Value = JsonDocument.Parse("{\"color\": \"transparent\"}")
        }
    };

    private static LeaveType holidayLeave = new LeaveType
    {
        Id = Guid.NewGuid(),
        Name = "urlop wypoczynkowy",
        Order = 1,
        Properties = new LeaveType.LeaveTypeProperties
        {
            DefaultLimit = workingHours * 26,
            IncludeFreeDays = false,
            Color = "#0137C9",
            Catalog = LeaveTypeCatalog.Holiday,
        }
    };

    private static LeaveType onDemandLeave = new LeaveType
    {
        Id = Guid.NewGuid(),
        Name = "urlop na żądanie",
        BaseLeaveTypeId = holidayLeave.Id,
        Order = 2,
        Properties = new LeaveType.LeaveTypeProperties
        {
            DefaultLimit = workingHours * 4,
            IncludeFreeDays = false,
            Color = "#B88E1E",
            Catalog = LeaveTypeCatalog.OnDemand,
        }
    };

    private static LeaveType sickLeave = new LeaveType
    {
        Id = Guid.NewGuid(),
        Name = "niezdolność do pracy z powodu choroby",
        Order = 3,
        Properties = new LeaveType.LeaveTypeProperties
        {
            IncludeFreeDays = true,
            Color = "#FF3333",
            Catalog = LeaveTypeCatalog.Sick,
        }
    };

    private static LeaveType saturdayLeave = new LeaveType
    {
        Id = Guid.NewGuid(),
        Name = "urlop za sobotę",
        Order = 14,
        Properties = new LeaveType.LeaveTypeProperties
        {
            DefaultLimit = workingHours,
            IncludeFreeDays = false,
            Color = "#FFFF33",
            Catalog = LeaveTypeCatalog.Saturday,
        }
    };

    public static async Task MigrateDb(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var services = scope.ServiceProvider;

            var dbContext = services.GetRequiredService<LeaveSystemDbContext>();
            if (dbContext == null)
            {
                throw new InvalidOperationException("Cannot find DB context.");
            }

            try
            {
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot migrate the database.", ex);
            }
        }
    }

    public static async Task FillInDatabase(this WebApplication app, IConfiguration configuration)
    {
        try
        {
            var cancellationToken = new CancellationToken();
            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection
                .AddServices(configuration)
                .AddScoped<CurrentDateService, CustomDateService>()
                .BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<LeaveSystemDbContext>();
            var graphUserService = serviceProvider.GetRequiredService<GetGraphUserService>();
            var saveGraphUserService = serviceProvider.GetRequiredService<SaveGraphUserService>();
            var graphUsers = await graphUserService.Get(cancellationToken);
            defaultUser = CreateFederatedUser(graphUsers, defaultUserMock.Id, defaultUserMock.Email,
                defaultUserMock.Name, defaultUserMock.Roles);
            testUsers = testUserMock
                .Select(t => CreateFederatedUser(graphUsers, t.Id, t.Email, t.Name, t.Roles))
                .Union(new FederatedUser[] { defaultUser })
                .ToArray();
            await FillInGraphUsers(graphUsers, saveGraphUserService, app.Logger, cancellationToken);
            await FillInSimpleData(dbContext, serviceProvider);
            await FillInEventSourcingData(serviceProvider);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Something went wrong when fill in database.");
        }
    }

    private static FederatedUser CreateFederatedUser(IEnumerable<FederatedUser> graphUsers, string id, string? email,
        string? name, IEnumerable<string> roles)
    {
        var graphUser = graphUsers.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase));
        return FederatedUser.Create(graphUser.Id ?? id, graphUser.Email ?? email, graphUser.Name ?? name, roles);
    }

    private static async Task FillInGraphUsers(IEnumerable<FederatedUser> graphUsers,
        SaveGraphUserService saveGraphUserService, ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var usersToAdd = testUsers.Where(u => !graphUsers.Any(g => u.Id == g.Id));
            foreach (var userToAdd in usersToAdd)
            {
                Guard.Against.Null(userToAdd.Email);
                await saveGraphUserService.Add(userToAdd.Email, userToAdd.Name, userToAdd.Roles, cancellationToken);
            }

            var usersToUpdate = testUsers.Where(u => IsRoleNotEqual(graphUsers, u));
            foreach (var userToUpdate in usersToUpdate)
            {
                await saveGraphUserService.Update(userToUpdate.Id, userToUpdate.Email, userToUpdate.Name,
                    userToUpdate.Roles, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Something went wrong when fill in users in the Azure.");
        }
    }

    private static bool IsRoleNotEqual(IEnumerable<FederatedUser> graphUsers, FederatedUser federatedUser)
    {
        var usersFound = graphUsers.Where(g => federatedUser.Id == g.Id);
        if (!usersFound.Any())
        {
            return false;
        }

        var userFound = usersFound.First();
        return !userFound.Roles.SequenceEqual(federatedUser.Roles);
    }

    private static async Task FillInSimpleData(LeaveSystemDbContext dbContext, IServiceProvider services)
    {
        await dbContext.FillInLeaveTypes();
        var currentDateService = services.GetRequiredService<CurrentDateService>();
        await dbContext.FillInUserLeaveLimits(currentDateService);
        await dbContext.FillInSettings();
        await dbContext.SaveChangesAsync();
    }

    private static async Task FillInEventSourcingData(IServiceProvider services)
    {
        var queryBus = services.GetRequiredService<IQueryBus>();
        var commandBus = services.GetRequiredService<ICommandBus>();
        var currentDateService = services.GetRequiredService<CurrentDateService>();
        await FillInWorkingHours(queryBus, commandBus, currentDateService);
        await FillInLeaveRequests(queryBus, commandBus, currentDateService);
    }

    private static async Task FillInWorkingHours(IQueryBus queryBus, ICommandBus commandBus, CurrentDateService currentDateService)
    {
        var workingHoursFromDb = await queryBus.Send<GetWorkingHours, IPagedList<WorkingHours>>(GetWorkingHours.Create(
            null, null, null, null,
            testUsers.Take(5).Select(u => u.Id).ToArray(), defaultUser,
            null));
        if (workingHoursFromDb.Any())
        {
            return;
        }
        var now = currentDateService.UtcNowWithoutTime();
        await CreateWorkingHours(
            commandBus,
            defaultUser.Id,
            DateTimeOffsetExtensions.CreateFromDate(now.Year - 1, 1, 1),
            null,
            TimeSpan.FromHours(8),
            defaultUser);
        await CreateWorkingHours(
            commandBus,
            testUsers[0].Id,
            DateTimeOffsetExtensions.CreateFromDate(now.Year - 1, 1, 1),
            null,
            TimeSpan.FromHours(8),
            defaultUser);
        await CreateWorkingHours(
            commandBus,
            testUsers[1].Id,
            DateTimeOffsetExtensions.CreateFromDate(now.Year - 5, 3, 1),
            DateTimeOffsetExtensions.CreateFromDate(now.Year + 2, 3, 1),
            TimeSpan.FromHours(8),
            defaultUser);
        await CreateWorkingHours(
            commandBus,
            testUsers[2].Id,
            DateTimeOffsetExtensions.CreateFromDate(now.Year - 2, 12, 1),
            DateTimeOffsetExtensions.CreateFromDate(now.Year + 4, 7, 10),
            TimeSpan.FromHours(4),
            defaultUser);
        await CreateWorkingHours(
            commandBus,
            testUsers[3].Id,
            DateTimeOffsetExtensions.CreateFromDate(now.Year - 1, 1, 1),
            DateTimeOffsetExtensions.CreateFromDate(now.Year + 5, 1, 1),
            TimeSpan.FromHours(8),
            defaultUser);
        await CreateWorkingHours(
            commandBus,
            testUsers[4].Id,
            DateTimeOffsetExtensions.CreateFromDate(now.Year - 8, 6, 9),
            DateTimeOffsetExtensions.CreateFromDate(now.Year - 3, 1, 1),
            TimeSpan.FromHours(8),
            defaultUser);
        await CreateWorkingHours(
            commandBus,
            testUsers[4].Id,
            DateTimeOffsetExtensions.CreateFromDate(now.Year - 3, 1, 2),
            DateTimeOffsetExtensions.CreateFromDate(now.Year + 1, 5, 6),
            TimeSpan.FromHours(4),
            defaultUser);
    }

    private static Task CreateWorkingHours(ICommandBus commandBus, string? userId, DateTimeOffset? dateFrom,
        DateTimeOffset? dateTo, TimeSpan? duration, FederatedUser? createdBy)
    {
        var workingHoursId = Guid.NewGuid();
        var command = EventSourcing.WorkingHours.CreatingWorkingHours.CreateWorkingHours.Create(workingHoursId, userId, dateFrom, dateTo, duration, createdBy);
        return commandBus.Send(command);
    }

    private static async Task FillInLeaveRequests(IQueryBus queryBus, ICommandBus commandBus, CurrentDateService currentDateService)
    {
        var pagedList = await queryBus.Send<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>(
            GetLeaveRequests.Create(
                null, null, null, null, null, null, null,
                testUsers.Take(5).Select(u => u.Id).ToArray(),
                defaultUser));
        if (pagedList.Any())
        {
            return;
        }

        await SetupUser0(commandBus, defaultUser, testUsers[0], currentDateService);
        await SetupUser1(commandBus, defaultUser, testUsers[1], currentDateService);
        await SetupUser2(commandBus, defaultUser, testUsers[2], currentDateService);
        await SetupUser3(commandBus, defaultUser, testUsers[3], currentDateService);
        await SetupUser4(commandBus, defaultUser, testUsers[4], currentDateService);
        await SetupUser4(commandBus, defaultUser, defaultUser, currentDateService);

        var now = currentDateService.UtcNowWithoutTime();
        int firstMonth = now.Month > 6 ? 1 : 9;
        int secondMonth = now.Month > 6 ? 5 : 12;
        await AddSaturdayLeaveRequest(commandBus, testUsers[0], now, firstMonth);
        await AddSaturdayLeaveRequest(commandBus, testUsers[1], now, secondMonth);
        await AddSaturdayLeaveRequest(commandBus, defaultUser, now, firstMonth);
        await AddOnDemandLeaveRequest(commandBus, defaultUser, now);
    }

    private static async Task AddOnDemandLeaveRequest(ICommandBus commandBus, FederatedUser user, DateTimeOffset date)
    {
        date = GetFirstWorkingDay(date);
        await CreateLeaveRequest(commandBus, date, date, onDemandLeave.Id, user);
    }

    private static async Task SetupUser4(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser, CurrentDateService currentDateService)
    {
        var now = currentDateService.UtcNowWithoutTime();
        var start = GetFirstWorkingDay(now.GetFirstDayOfYear());
        var end = start;

        var leaveRequestId = await CreateLeaveRequest(commandBus, start, end, sickLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);

        start = GetFirstWorkingDay(new DateTimeOffset(now.Year, 12, 10, 0, 0, 0, TimeSpan.Zero));
        end = start;

        leaveRequestId = await CreateLeaveRequest(commandBus, start, end, sickLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);
    }

    private static async Task SetupUser3(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser, CurrentDateService currentDateService)
    {
        var now = currentDateService.UtcNowWithoutTime();
        var start = GetFirstWorkingDay(now.GetFirstDayOfYear());
        var end = GetFirstWorkingDay(new DateTimeOffset(now.Year, 12, 20, 0, 0, 0, TimeSpan.Zero));

        var leaveRequestId = await CreateLeaveRequest(commandBus, start, end, sickLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);
    }

    private static async Task SetupUser2(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser, CurrentDateService currentDateService)
    {
        var now = currentDateService.UtcNowWithoutTime();
        var start = GetFirstWorkingDay(now);
        var end = start;

        var leaveRequestId = await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);
    }

    private static async Task SetupUser1(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser, CurrentDateService currentDateService)
    {
        var now = currentDateService.UtcNowWithoutTime();
        var start = GetFirstWorkingDay(now.AddDays(-5));
        var end = GetFirstWorkingDay(now.AddDays(2));

        var leaveRequestId = await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);

        start = GetFirstWorkingDay(now.AddDays(8));
        end = start;
        leaveRequestId = await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);
        await RejectLeaveRequest(commandBus, leaveRequestId, defaultUser);

        start = GetFirstWorkingDay(now.AddDays(10));
        end = start;
        leaveRequestId = await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);
        await CancelLeaveRequest(commandBus, leaveRequestId, testUser);

        start = GetFirstWorkingDay(now.AddDays(8));
        end = GetFirstWorkingDay(now.AddDays(10));
        await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);

        start = now.AddDays(14);
        end = now.AddDays(18);
        leaveRequestId = await CreateLeaveRequest(commandBus, start, end, sickLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);
    }

    private static async Task SetupUser0(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser, CurrentDateService currentDateService)
    {
        var now = currentDateService.UtcNowWithoutTime();
        var start = GetFirstWorkingDay(now);
        var end = GetFirstWorkingDay(now.AddDays(7));

        var leaveRequestId = await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);

        start = GetFirstWorkingDay(now.AddDays(-14));
        end = start;
        leaveRequestId = await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);
        await RejectLeaveRequest(commandBus, leaveRequestId, defaultUser);

        start = GetFirstWorkingDay(now.AddDays(14));
        end = start;
        leaveRequestId = await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);
        await CancelLeaveRequest(commandBus, leaveRequestId, testUser);

        start = GetFirstWorkingDay(now.AddDays(-15));
        end = GetFirstWorkingDay(now.AddDays(-10));
        await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);

        start = now.AddDays(8);
        end = now.AddDays(30);
        await CreateLeaveRequest(commandBus, start, end, sickLeave.Id, testUser);

        start = GetFirstWorkingDay(now.AddDays(-7));
        end = start;
        await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser, TimeSpan.FromHours(4));
    }

    private static async Task AddSaturdayLeaveRequest(ICommandBus commandBus, FederatedUser testUser,
        DateTimeOffset now, int month)
    {
        var startDay = GetFirstWorkingDay(new DateTimeOffset(now.Year, month, 10, 0, 0, 0, TimeSpan.Zero));
        var endDay = startDay;
        await CreateLeaveRequest(commandBus, startDay, endDay, saturdayLeave.Id, testUser);
    }

    private static async Task CancelLeaveRequest(ICommandBus commandBus, Guid leaveRequestId, FederatedUser canceledBy)
    {
        var command = EventSourcing.LeaveRequests.CancelingLeaveRequest.CancelLeaveRequest.Create(
            leaveRequestId,
            "",
            canceledBy
        );
        await commandBus.Send(command);
    }

    private static async Task AcceptLeaveRequest(ICommandBus commandBus, Guid leaveRequestId, FederatedUser acceptedBy)
    {
        var command = EventSourcing.LeaveRequests.AcceptingLeaveRequest.AcceptLeaveRequest.Create(
            leaveRequestId,
            "",
            acceptedBy
        );
        await commandBus.Send(command);
    }

    private static async Task RejectLeaveRequest(ICommandBus commandBus, Guid leaveRequestId, FederatedUser rejectedBy)
    {
        var command = EventSourcing.LeaveRequests.RejectingLeaveRequest.RejectLeaveRequest.Create(
            leaveRequestId,
            "",
            rejectedBy
        );
        await commandBus.Send(command);
    }

    private static DateTimeOffset GetFirstWorkingDay(DateTimeOffset now)
    {
        for (; DateCalculator.GetDayKind(now) != DateCalculator.DayKind.WORKING; now = now.AddDays(2))
        {
        }

        return now;
    }

    private static async Task<Guid> CreateLeaveRequest(ICommandBus commandBus,
        DateTimeOffset dateFrom,
        DateTimeOffset dateTo,
        Guid leaveTypeId,
        FederatedUser createdBy,
        TimeSpan? duration = null)
    {
        var leaveRequestId = Guid.NewGuid();
        var command = EventSourcing.LeaveRequests.CreatingLeaveRequest.CreateLeaveRequest.Create(
            leaveRequestId,
            dateFrom,
            dateTo,
            duration,
            leaveTypeId,
            null,
            createdBy
        );
        await commandBus.Send(command);
        return leaveRequestId;
    }

    private static async Task FillInUserLeaveLimits(this LeaveSystemDbContext dbContext, CurrentDateService currentDateService)
    {
        if (await dbContext.UserLeaveLimits.AnyAsync())
        {
            return;
        }

        var now = currentDateService.UtcNowWithoutTime();
        var holidayLimits = testUsers.Select(u => new UserLeaveLimit
        {
            LeaveTypeId = holidayLeave.Id,
            Limit = holidayLeave.Properties?.DefaultLimit,
            AssignedToUserId = u.Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        });
        await dbContext.UserLeaveLimits.AddRangeAsync(holidayLimits);
        var onDemandLimits = testUsers.Select(u => new UserLeaveLimit
        {
            LeaveTypeId = onDemandLeave.Id,
            Limit = onDemandLeave.Properties?.DefaultLimit,
            AssignedToUserId = u.Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        });
        await dbContext.UserLeaveLimits.AddRangeAsync(onDemandLimits);
        int firstMonth = now.Month > 6 ? 1 : 9;
        int secondMonth = now.Month > 6 ? 5 : 12;
        var saturdayFirsLimits = GetSaturdayLimits(now, firstMonth);
        await dbContext.UserLeaveLimits.AddRangeAsync(saturdayFirsLimits);
        var saturdaySecondLimits = GetSaturdayLimits(now, secondMonth);
        await dbContext.UserLeaveLimits.AddRangeAsync(saturdaySecondLimits);
        await dbContext.UserLeaveLimits.AddAsync(new UserLeaveLimit
        {
            LeaveTypeId = sickLeave.Id,
        });
    }

    private static IEnumerable<UserLeaveLimit> GetSaturdayLimits(DateTimeOffset now, int month)
    {
        var firstDayOfMonth = new DateTimeOffset(now.Year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var lastDayOfMonth = new DateTimeOffset(now.Year, month, 30, 23, 59, 59, TimeSpan.Zero);
        return testUsers.Select(u => new UserLeaveLimit
        {
            LeaveTypeId = saturdayLeave.Id,
            Limit = saturdayLeave.Properties?.DefaultLimit,
            AssignedToUserId = u.Id,
            ValidSince = firstDayOfMonth,
            ValidUntil = lastDayOfMonth,
            Property = new UserLeaveLimit.UserLeaveLimitProperties
            {
                Description = $"2022-{month}-10"
            }
        });
    }

    private static async Task FillInLeaveTypes(this LeaveSystemDbContext dbContext)
    {
        if (await dbContext.LeaveTypes.AnyAsync())
        {
            holidayLeave = await dbContext.LeaveTypes.FirstOrDefaultAsync(l => l.Order == 1) ?? holidayLeave;
            onDemandLeave = await dbContext.LeaveTypes.FirstOrDefaultAsync(l => l.Order == 2) ?? holidayLeave;
            sickLeave = await dbContext.LeaveTypes.FirstOrDefaultAsync(l => l.Order == 3) ?? sickLeave;
            saturdayLeave = await dbContext.LeaveTypes.FirstOrDefaultAsync(l => l.Order == 14) ?? saturdayLeave;
            return;
        }

        var leaveTypes = new[]
        {
            holidayLeave,
            onDemandLeave,
            sickLeave,
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop okolicznościowy",
                Order = 4,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#30D5C8" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop wychowawczy",
                Order = 5,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#2EB82E" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop macierzyński",
                Order = 6,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#FF99DD" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop bezpłatny",
                Order = 7,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#0033CC" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "opieka nad chorym dzieckiem lub innym członkiem rodziny",
                Order = 8,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#FF3333" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "nieobecność usprawiedliwiona płatna",
                Order = 9,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#80AAFF" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "nieobecność nieusprawiedliwiona",
                Order = 10,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#0066FF" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "opieka nad dzieckiem do 14 lat - art. 188 KP",
                Order = 11,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#FF4DA6" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop ojcowski",
                Order = 12,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#FF99DD" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop tacierzyński",
                Order = 13,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "#FF99DD" }
            },
            saturdayLeave,
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop od firmy",
                Order = 15,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "#0137C9" }
            },
            new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop szkoleniowy",
                Order = 16,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "blue" }
            }
        };
        await dbContext.LeaveTypes.AddRangeAsync(leaveTypes);
    }

    private static async Task FillInSettings(this LeaveSystemDbContext dbContext)
    {
        if (await dbContext.Settings.AnyAsync())
        {
            return;
        }

        await dbContext.Settings.AddRangeAsync(settings);
    }
}