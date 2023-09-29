using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class LeaveTypesControllerGetTest
{
    private LeaveTypesController GetSut(LeaveSystemDbContext dbContext)
    {
        return new LeaveTypesController(dbContext);
    }

    [Fact]
    public async Task WhenGettingSet_ThenReturnThisSet()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = GetSut(dbContext);
        //When
        var set = sut.Get();
        //Then
        set.Should().BeEquivalentTo(dbContext.Set<LeaveType>());
    }
}