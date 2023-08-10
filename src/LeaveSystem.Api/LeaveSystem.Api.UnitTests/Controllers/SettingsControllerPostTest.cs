using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class SettingsControllerPostTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new SettingsController(dbContext);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        //When
        var result = await sut.Post(FakeSettingsProvider.GetAcceptedSetting());
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task WhenModelStateIsValid_ThenReturnCreated()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new SettingsController(dbContext);
        var acceptedSetting = FakeSettingsProvider.GetAcceptedSetting();
        //When
        var result = await sut.Post(acceptedSetting);
        //Then
        result.Should().BeOfType<CreatedODataResult<Setting>>();
        sut.Get().Should().ContainEquivalentOf(acceptedSetting,
            o => o.ComparingByMembers<JsonElement>());
    }
}