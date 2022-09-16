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
        LeaveTypeId = Guid.NewGuid(),
        Name = "urlop wypoczynkowy",
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
                throw new InvalidOperationException("Cannot find DB context. Please fix the bug.");
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
            LeaveTypeId = holiday.LeaveTypeId,
            Limit = holiday.Properties?.DefaultLimit,
            AssignedToUserEmail = DefaultUserEmail,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        });
    }

    public static void FillInRoles(this LeaveSystemDbContext dbContext)
    {
        dbContext.Roles.Add(new Role { Email = DefaultUserEmail, RoleId = Guid.NewGuid(), RoleType = RoleType.GlobalAdmin });
    }

    private static void FillInLeaveTypes(this LeaveSystemDbContext dbContext)
    {
        dbContext.LeaveTypes.Add(holiday);
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop na żądanie",
            BaseLeaveTypeId = holiday.Id,
            Properties = new LeaveType.LeaveTypeProperties { DefaultLimit = TimeSpan.FromDays(4), IncludeFreeDays = false, Color = "yellow" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "niezdolność do pracy z powodu choroby",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop okolicznościowy",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop wychowawczy",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop macierzyński",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop bezpłatny",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "opieka nad chorym dzieckiem lub innym członkiem rodziny",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "nieobecność usprawiedliwiona płatna",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "nieobecność nieusprawiedliwiona",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "opieka nad dzieckiem do 14 lat - art. 188 KP",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop ojcowski",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop tacierzyński",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = true, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop za sobotę",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop od firmy",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "red" }
        });
        dbContext.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = Guid.NewGuid(),
            Name = "urlop szkoleniowy",
            Properties = new LeaveType.LeaveTypeProperties { IncludeFreeDays = false, Color = "red" }
        });
    }
}
