using LeaveSystem.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using LeaveSystem.UnitTests.Providers;

namespace LeaveSystem.UnitTests;

public static class DbContextFactory
{
    public static async Task<LeaveSystemDbContext> CreateDbContextAsync()
    {
        var builder = new DbContextOptionsBuilder<LeaveSystemDbContext>();

        // Create a fresh service provider, and therefore a fresh
        // InMemory database instance.
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider();
        // Use In-Memory DB.
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseInternalServiceProvider(serviceProvider);

        var dbContext = new LeaveSystemDbContext(builder.Options);
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }
    
    public static async Task<LeaveSystemDbContext> CreateAndFillDbAsync()
    {
        var dbContext = await CreateDbContextAsync();
        await AddLeaveTypesToDbAsync(dbContext);
        await dbContext.SaveChangesAsync();
        await AddUserLeaveLimitsToDbAsync(dbContext);
        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    private static async Task AddLeaveTypesToDbAsync(LeaveSystemDbContext dbContext)
    {
        await dbContext.LeaveTypes.AddRangeAsync(
            FakeLeaveTypeProvider.GetLeaveTypes()
        );
    }

    private static async Task AddUserLeaveLimitsToDbAsync(LeaveSystemDbContext dbContext)
    {
        await dbContext.UserLeaveLimits.AddRangeAsync(FakeUserLeaveLimitProvider.GetLimits());
    }
}
