using GoldenEye.Commands;
using GoldenEye.Queries;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using Marten.Pagination;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Api.Db;
public static class DbContextExtenstions
{
    private class DbContextExtenstionsLogger { }
    private static readonly TimeSpan workingHours = TimeSpan.FromHours(8);
    private const string DefaultUserEmail = "karolbr5@gmail.com";
    private static readonly FederatedUser defaultUser = new FederatedUser(DefaultUserEmail, "Karol Volt");
    private static readonly FederatedUser[] testUsers = new[]
    {
        new FederatedUser("jkowalski@test.com", "Jan Kowalski"),
        new FederatedUser("mnowak@test.com", "Maria Nowak"),
        new FederatedUser("jszczepanek@test.com", "Jadwiga Szczepanek"),
        new FederatedUser("aszewczyk@test.com", "Aleksandra Szewczyk"),
        new FederatedUser("ourbanek@test.com", "Olgierd Urbanek"),
        defaultUser
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
            Color = "blue",
            IsDefault = true,
        }
    };
    private static LeaveType onDemandLeave = new LeaveType
    {
        Id = Guid.NewGuid(),
        Name = "urlop na żądanie",
        BaseLeaveTypeId = holidayLeave.Id,
        Order = 2,
        Properties = new LeaveType.LeaveTypeProperties { DefaultLimit = workingHours * 4, IncludeFreeDays = false, Color = "yellow" }
    };
    private static LeaveType sickLeave = new LeaveType
    {
        Id = Guid.NewGuid(),
        Name = "niezdolność do pracy z powodu choroby",
        Order = 3,
        Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
    };
    private static LeaveType saturdayLeave = new LeaveType
    {
        Id = Guid.NewGuid(),
        Name = "urlop za sobotę",
        Order = 14,
        Properties = new LeaveType.LeaveTypeProperties { DefaultLimit = workingHours, IncludeFreeDays = false, Color = "red" }
    };
    public static void MigrateDb(this IApplicationBuilder app)
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
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot migrate the database.", ex);
            }
        }
    }

    public static async Task FillInDatabase(this IApplicationBuilder app)
    {
        await using (var scope = app.ApplicationServices.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var dbContext = services.GetRequiredService<LeaveSystemDbContext>();
                await FillInSimpleData(dbContext);
                await FillInLeaveRequests(services);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<DbContextExtenstionsLogger>>();
                logger.LogError(ex, "Something wrong happened when fill in database.");
            }
        }
    }

    private static async Task FillInSimpleData(LeaveSystemDbContext dbContext)
    {
        await dbContext.FillInLeaveTypes();
        await dbContext.FillInRoles();
        await dbContext.FillInUserLeaveLimit();
        await dbContext.SaveChangesAsync();
    }

    private static async Task FillInLeaveRequests(IServiceProvider services)
    {
        var queryBus = services.GetRequiredService<IQueryBus>();
        var commandBus = services.GetRequiredService<ICommandBus>();
        var pagedList = await queryBus.Send<GetLeaveRequests, IPagedList<LeaveRequestShortInfo>>(GetLeaveRequests.Create(
                   null, null, null, null, null, null,
                   testUsers.Take(5).Select(u => u.Email).ToArray(),
                   defaultUser));
        if (pagedList.Any())
        {
            return;
        }
        await SetupUser0(commandBus, defaultUser, testUsers[0]);
        await SetupUser1(commandBus, defaultUser, testUsers[1]);
        await SetupUser2(commandBus, defaultUser, testUsers[2]);
        await SetupUser3(commandBus, defaultUser, testUsers[3]);
        await SetupUser4(commandBus, defaultUser, testUsers[4]);
        await SetupUser4(commandBus, defaultUser, defaultUser);

        var now = DateTimeOffset.UtcNow;
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

    private static async Task SetupUser4(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser)
    {
        var now = DateTimeOffset.UtcNow;
        var start = GetFirstWorkingDay(now.GetFirstDayOfYear());
        var end = start;

        var leaveRequestId = await CreateLeaveRequest(commandBus, start, end, sickLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);

        start = GetFirstWorkingDay(new DateTimeOffset(now.Year, 12, 10, 0, 0, 0, TimeSpan.Zero));
        end = start;

        leaveRequestId = await CreateLeaveRequest(commandBus, start, end, sickLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);
    }

    private static async Task SetupUser3(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser)
    {
        var now = DateTimeOffset.UtcNow;
        var start = GetFirstWorkingDay(now.GetFirstDayOfYear());
        var end = GetFirstWorkingDay(new DateTimeOffset(now.Year, 12, 20, 0, 0, 0, TimeSpan.Zero));

        var leaveRequestId = await CreateLeaveRequest(commandBus, start, end, sickLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);
    }

    private static async Task SetupUser2(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser)
    {
        var now = DateTimeOffset.UtcNow;
        var start = GetFirstWorkingDay(now);
        var end = start;

        var leaveRequestId = await CreateLeaveRequest(commandBus, start, end, holidayLeave.Id, testUser);
        await AcceptLeaveRequest(commandBus, leaveRequestId, defaultUser);
    }

    private static async Task SetupUser1(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser)
    {
        var now = DateTimeOffset.UtcNow;
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

    private static async Task SetupUser0(ICommandBus commandBus, FederatedUser defaultUser, FederatedUser testUser)
    {
        var now = DateTimeOffset.UtcNow;
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

    private static async Task AddSaturdayLeaveRequest(ICommandBus commandBus, FederatedUser testUser, DateTimeOffset now, int month)
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
        for (; DateCalculator.GetDayKind(now) != DateCalculator.DayKind.WORKING; now = now.AddDays(2)) { }
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

    private static async Task FillInUserLeaveLimit(this LeaveSystemDbContext dbContext)
    {
        if (await dbContext.UserLeaveLimits.AnyAsync())
        {
            return;
        }
        var now = DateTimeOffset.UtcNow;
        var holidayLimits = testUsers.Select(u => new UserLeaveLimit
        {
            LeaveTypeId = holidayLeave.Id,
            Limit = holidayLeave.Properties?.DefaultLimit,
            AssignedToUserEmail = u.Email,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        });
        await dbContext.UserLeaveLimits.AddRangeAsync(holidayLimits);
        var onDemandLimits = testUsers.Select(u => new UserLeaveLimit
        {
            LeaveTypeId = onDemandLeave.Id,
            Limit = onDemandLeave.Properties?.DefaultLimit,
            AssignedToUserEmail = u.Email,
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
            AssignedToUserEmail = u.Email,
            ValidSince = firstDayOfMonth,
            ValidUntil = lastDayOfMonth,
            Property = new UserLeaveLimit.UserLeaveLimitProperties
            {
                Description = $"2022-{month}-10"
            }
        });
    }

    private static async Task FillInRoles(this LeaveSystemDbContext dbContext)
    {
        if (await dbContext.Roles.AnyAsync())
        {
            return;
        }
        await dbContext.Roles.AddAsync(new Role { Email = DefaultUserEmail, Id = Guid.NewGuid(), RoleType = RoleType.GlobalAdmin });
        var roles = testUsers.Select(u => new Role { Email = u.Email, Id = Guid.NewGuid(), RoleType = RoleType.Employee });
        await dbContext.Roles.AddRangeAsync(roles);
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
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop wychowawczy",
                Order = 5,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop macierzyński",
                Order = 6,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop bezpłatny",
                Order = 7,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "opieka nad chorym dzieckiem lub innym członkiem rodziny",
                Order = 8,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "nieobecność usprawiedliwiona płatna",
                Order = 9,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "nieobecność nieusprawiedliwiona",
                Order = 10,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "opieka nad dzieckiem do 14 lat - art. 188 KP",
                Order = 11,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop ojcowski",
                Order = 12,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop tacierzyński",
                Order = 13,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
            },
                saturdayLeave,
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop od firmy",
                Order = 15,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "red" }
            },
                new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = "urlop szkoleniowy",
                Order = 16,
                Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "red" }
            }
        };
        await dbContext.LeaveTypes.AddRangeAsync(leaveTypes);
    }
}
