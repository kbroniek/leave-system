using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class SettingsControllerDeleteTest
{
    [Theory]
    [MemberData(nameof(Get_FakeEntities_TestData))]
    public async Task WhenEntityWithProvidedIdNotExistsInSet_ThenReturnNotFound(IEnumerable<Setting> fakeEntitiesFromDb)
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.Settings.AddRangeAsync(fakeEntitiesFromDb);
        await dbContext.SaveChangesAsync();
        var sut = new SettingsController(dbContext);
        var fakeId = Guid.NewGuid().ToString();
        //When
        var result = await sut.DeleteAsync(fakeId);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }

    public static IEnumerable<object[]> Get_FakeEntities_TestData()
    {
        yield return new object[] { Enumerable.Empty<Setting>() };
        yield return new object[] { FakeSettingsProvider.GetSettings() };
    }
    
    [Fact]
    public async Task WhenEntityWithProvidedIdExistsInSet_ThenRemoveFromSetAndReturnNoContent()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.Settings.AddRangeAsync(FakeSettingsProvider.GetSettings());
        await dbContext.SaveChangesAsync();
        var sut = new SettingsController(dbContext);
        var fakeId = FakeSettingsProvider.CanceledSettingId;
        //When
        var result = await sut.DeleteAsync(fakeId);
        //Then
        result.Should().BeOfType<NoContentResult>();
        sut.Get().Should().BeEquivalentTo(new[]
        {
            FakeSettingsProvider.GetAcceptedSetting(), 
            FakeSettingsProvider.GetPendingSetting(),
            FakeSettingsProvider.GetRejectedSetting()
        }, o => o.ComparingByMembers<JsonElement>());
    }
}