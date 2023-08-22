using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class UserLeaveLimitsControllerDeleteTest
{
    [Theory]
    [MemberData(nameof(Get_FakeEntities_TestData))]
    public async Task WhenEntityWithProvidedIdNotExistsInSet_ThenReturnNotFound(IEnumerable<UserLeaveLimit> fakeEntitiesFromDb)
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.UserLeaveLimits.AddRangeAsync(fakeEntitiesFromDb);
        await dbContext.SaveChangesAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        var fakeId = Guid.NewGuid();
        //When
        var result = await sut.Delete(fakeId);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }

    public static IEnumerable<object[]> Get_FakeEntities_TestData()
    {
        yield return new object[] { Enumerable.Empty<UserLeaveLimit>() };
        yield return new object[] { FakeUserLeaveLimitProvider.GetLimits() };
    }
    
    [Fact]
    public async Task WhenEntityWithProvidedIdExistsInSet_ThenRemoveFromSetAndReturnNoContent()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.UserLeaveLimits.AddRangeAsync(FakeUserLeaveLimitProvider.GetLimits());
        await dbContext.SaveChangesAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        var fakeId = FakeUserLeaveLimitProvider.FakeLimitForOnDemandLeaveId;
        //When
        var result = await sut.Delete(fakeId);
        //Then
        result.Should().BeOfType<NoContentResult>();
        sut.Get().Should().BeEquivalentTo(new[]
        {
            FakeUserLeaveLimitProvider.GetLimitForHolidayLeave(), 
            FakeUserLeaveLimitProvider.GetLimitForSickLeave(),
        });
    }
}