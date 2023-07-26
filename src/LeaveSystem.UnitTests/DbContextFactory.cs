using LeaveSystem.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
}
