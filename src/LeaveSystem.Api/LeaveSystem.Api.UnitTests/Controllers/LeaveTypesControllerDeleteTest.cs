using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class LeaveTypesControllerDeleteTest
{
    [Theory]
    [MemberData(nameof(Get_FakeEntities_TestData))]
    public async Task WhenEntityWithProvidedIdNotExistsInSet_ThenReturnNotFound(IEnumerable<LeaveType> fakeEntitiesFromDb)
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddRangeAsync(fakeEntitiesFromDb);
        await dbContext.SaveChangesAsync();
        var sut = new LeaveTypesController(dbContext);
        var fakeId = Guid.NewGuid();
        //When
        var result = await sut.Delete(fakeId);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }

    public static IEnumerable<object[]> Get_FakeEntities_TestData()
    {
        yield return new object[] { Enumerable.Empty<LeaveType>() };
        yield return new object[] { FakeLeaveTypeProvider.GetLeaveTypes() };
    }
    
    [Fact]
    public async Task WhenEntityWithProvidedIdExistsInSet_ThenRemoveFromSetAndReturnNoContent()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddRangeAsync(FakeLeaveTypeProvider.GetLeaveTypes());
        await dbContext.SaveChangesAsync();
        var sut = new LeaveTypesController(dbContext);
        var fakeId = FakeLeaveTypeProvider.FakeSickLeaveId;
        //When
        var result = await sut.Delete(fakeId);
        //Then
        result.Should().BeOfType<NoContentResult>();
        var leaveTypesLeft = sut.Get().ToList();
        leaveTypesLeft.Should().BeEquivalentTo(new[]
        {
            FakeLeaveTypeProvider.GetFakeHolidayLeave(), FakeLeaveTypeProvider.GetFakeOnDemandLeave()
        });
    }
}