using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class UserLeaveLimitsControllerGetSingleResultTest
{
    [Fact]
    public async Task WhenNoUserLeaveLimitWithProvidedId_ThenReturnEmptyResult()
    {
        //Given
        var fakeLimits = FakeUserLeaveLimitProvider.GetLimits();
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.UserLeaveLimits.AddRangeAsync(fakeLimits);
        var fakeId = Guid.NewGuid();
        var sut = new UserLeaveLimitsController(dbContext);
        //When
        var result = sut.Get(fakeId);
        //Then
        result.Queryable.Should().BeEquivalentTo(
            Enumerable.Empty<UserLeaveLimit>()
        );
    }
    
    [Fact]
    public async Task WhenLeaveTypeWithThisIdExists_ThenReturnResult()
    {
        //Given
        var fakeUserLeaveLimits = FakeUserLeaveLimitProvider.GetLimits();
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.UserLeaveLimits.AddRangeAsync(fakeUserLeaveLimits);
        await dbContext.SaveChangesAsync();
        var fakeId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        var sut = new UserLeaveLimitsController(dbContext);
        //When
        var result = sut.Get(fakeId);
        //Then
        result.Queryable.Should().BeEquivalentTo(
            new [] {FakeUserLeaveLimitProvider.GetLimitForHolidayLeave()}
        );
    }
}