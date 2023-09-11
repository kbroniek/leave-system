using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class LeaveTypesControllerPostTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        //When
        var result = await sut.Post(FakeLeaveTypeProvider.GetFakeHolidayLeave());
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async Task WhenModelStateIsValid_ThenReturnCreated()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        var holidayLeave = FakeLeaveTypeProvider.GetFakeHolidayLeave();
        //When
        var result = await sut.Post(holidayLeave);
        //Then
        result.Should().BeOfType<CreatedODataResult<LeaveType>>();
        sut.Get().Should().ContainEquivalentOf(holidayLeave);
    }
}