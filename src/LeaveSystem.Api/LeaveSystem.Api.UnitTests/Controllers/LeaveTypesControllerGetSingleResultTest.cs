using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class LeaveTypesControllerGetSingleResultTest
{
    [Fact]
    public async Task WhenNoLeaveTypeWithProvidedId_ThenReturnEmptyResult()
    {
        //Given
        var fakeLeaveTypes = FakeLeaveTypeProvider.GetLeaveTypes();
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.AddRangeAsync(fakeLeaveTypes);
        var fakeId = Guid.NewGuid();
        var sut = new LeaveTypesController(dbContext);
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
        var fakeLeaveTypes = FakeLeaveTypeProvider.GetLeaveTypes();
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddRangeAsync(fakeLeaveTypes);
        await dbContext.SaveChangesAsync();
        var fakeId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid;
        var sut = new LeaveTypesController(dbContext);
        //When
        var result = sut.Get(fakeId);
        //Then
        result.Queryable.Should().BeEquivalentTo(
            new [] {FakeLeaveTypeProvider.GetFakeHolidayLeave()}
        );
    }
}