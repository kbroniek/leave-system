using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class UserLeaveLimitsControllerPostTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        //When
        var result = await sut.Post(FakeUserLeaveLimitProvider.GetLimitForHolidayLeave());
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async Task WhenModelStateIsValid_ThenCreateAndReturnCreated()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        var limitForHolidayLeave = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        //When
        var result = await sut.Post(limitForHolidayLeave);
        //Then
        result.Should().BeOfType<CreatedODataResult<UserLeaveLimit>>();
        sut.Get().Should().ContainEquivalentOf(limitForHolidayLeave);
    }
}