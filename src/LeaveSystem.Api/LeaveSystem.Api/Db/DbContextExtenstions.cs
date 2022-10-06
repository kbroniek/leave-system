using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using Microsoft.EntityFrameworkCore;

namespace LeaveSystem.Api.Db;
public static class DbContextExtenstions
{
    private const string DefaultUserEmail = "karolbr5@gmail.com";
    private static LeaveType holiday = new LeaveType
    {
        Id = Guid.NewGuid(),
        Name = "urlop wypoczynkowy",
        Order = 1,
        Properties = new LeaveType.LeaveTypeProperties { DefaultLimit = TimeSpan.FromDays(26), IncludeFreeDays = false, Color = "blue" }
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

    public static void FillInDatabase(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var services = scope.ServiceProvider;

            var dbContext = services.GetRequiredService<LeaveSystemDbContext>();
            if (!dbContext.LeaveTypes.Any())
            {
                dbContext.FillInLeaveTypes();
            }
            if (!dbContext.Roles.Any())
            {
                dbContext.FillInRoles();
            }
            if (!dbContext.UserLeaveLimits.Any())
            {
                dbContext.FillInUserLeaveLimit();
            }
            dbContext.SaveChanges();
        }
    }

    public static void FillInUserLeaveLimit(this LeaveSystemDbContext dbContext)
    {
        var now = DateTimeOffset.UtcNow;
        dbContext.UserLeaveLimits.Add(new UserLeaveLimit
        {
            LeaveTypeId = holiday.Id,
            Limit = holiday.Properties?.DefaultLimit,
            AssignedToUserEmail = DefaultUserEmail,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        });
    }

    public static void FillInRoles(this LeaveSystemDbContext dbContext)
    {
        dbContext.Roles.Add(new Role { Email = DefaultUserEmail, Id = Guid.NewGuid(), RoleType = RoleType.GlobalAdmin });
    }

    private static void FillInLeaveTypes(this LeaveSystemDbContext dbContext)
    {
        dbContext.LeaveTypes.Add(holiday);
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop na żądanie",
            BaseLeaveTypeId = holiday.Id,
            Order = 2,
            Properties = new LeaveType.LeaveTypeProperties { DefaultLimit = TimeSpan.FromDays(4), IncludeFreeDays = false, Color = "yellow" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "niezdolność do pracy z powodu choroby",
            Order = 3,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop okolicznościowy",
            Order = 4,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop wychowawczy",
            Order = 5,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop macierzyński",
            Order = 6,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop bezpłatny",
            Order = 7,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "opieka nad chorym dzieckiem lub innym członkiem rodziny",
            Order = 8,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "nieobecność usprawiedliwiona płatna",
            Order = 9,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "nieobecność nieusprawiedliwiona",
            Order = 10,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "opieka nad dzieckiem do 14 lat - art. 188 KP",
            Order = 11,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop ojcowski",
            Order = 12,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop tacierzyński",
            Order = 13,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop za sobotę",
            Order = 14,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop od firmy",
            Order = 15,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            Id = Guid.NewGuid(),
            Name = "urlop szkoleniowy",
            Order = 16,
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "red" }
        });
    }
}
