using LeaveSystem.Db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LeaveSystem.UnitTests;

public static class DbContextFactory
{
    public static async ValueTask<LeaveSystemDbContext> CreateDbContextAsync()
    {
        var builder = new DbContextOptionsBuilder<LeaveSystemDbContext>();

        // Use In-Memory DB.
        builder.UseInMemoryDatabase(Guid.NewGuid().ToString());

        var dbContext = new LeaveSystemDbContext(builder.Options);

        await dbContext.Database.EnsureCreatedAsync();

        return dbContext;
    }
}

