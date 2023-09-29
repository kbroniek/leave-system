using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.UnitTests.TestExtensions;
using LeaveSystem.Db;
using LeaveSystem.UnitTests;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class SettingsControllerGetTest
{
    private SettingsController GetSut(LeaveSystemDbContext dbContext)
    {
        return new SettingsController(dbContext);
    }

    [Fact]
    public async Task WhenGettingSet_ThenReturnThisSet()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = GetSut(dbContext);
        //When
        //Then
        await sut.CheckGetMethodAsync(dbContext);
    }
}