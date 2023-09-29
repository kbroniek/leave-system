using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class SettingsControllerGetSingleResultTest
{
    [Fact]
    public async Task WhenNoSettingWithProvidedId_ThenReturnEmptyResult()
    {
        //Given
        var fakeLeaveTypes = FakeSettingsProvider.GetSettings();
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.Settings.AddRangeAsync(fakeLeaveTypes);
        var fakeId = Guid.NewGuid().ToString();
        var sut = new SettingsController(dbContext);
        //When
        var result = sut.Get(fakeId);
        //Then
        result.Queryable.Should().BeEquivalentTo(
            Enumerable.Empty<LeaveType>()
        );
    }
    
    [Fact]
    public async Task WhenLeaveTypeWithThisIdExists_ThenReturnResult()
    {
        //Given
        var fakeLeaveTypes = FakeSettingsProvider.GetSettings();
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.Settings.AddRangeAsync(fakeLeaveTypes);
        await dbContext.SaveChangesAsync();
        var fakeId = FakeSettingsProvider.AcceptedSettingId;
        var sut = new SettingsController(dbContext);
        //When
        var result = sut.Get(fakeId);
        //Then
        result.Queryable.Should().BeEquivalentTo(
            new [] {FakeSettingsProvider.GetAcceptedSetting()}
            , o => o.ComparingByMembers<JsonElement>()
        );
    }
}