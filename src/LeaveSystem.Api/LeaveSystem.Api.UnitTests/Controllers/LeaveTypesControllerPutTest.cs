using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.UnitTests.TestExtensions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MockQueryable.Moq;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class LeaveTypesControllerPutTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        var fakeLeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId;
        var fakeLeaveType = FakeLeaveTypeProvider.GetFakeOnDemandLeave();
        //When
        var result = await sut.Put(fakeLeaveTypeId, fakeLeaveType);
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task WhenProvidedIdIsDifferentThanEntityId_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        var fakeLeaveTypeId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid;
        var fakeLeaveType = FakeLeaveTypeProvider.GetFakeOnDemandLeave();
        //When
        var result = await sut.Put(fakeLeaveTypeId, fakeLeaveType);
        //Then
        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task WhenProvidedLeaveTypeNotExistsInDatabase_ThenReturnNotFound()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        var fakeLeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId;
        var fakeLeaveType = FakeLeaveTypeProvider.GetFakeOnDemandLeave();
        //When
        var result = await sut.Put(fakeLeaveTypeId, fakeLeaveType);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndProductExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var holidayLeaveGuid = FakeLeaveTypeProvider.FakeHolidayLeaveGuid;
        var fakeLeaveTypeFromDb = FakeLeaveTypeProvider.GetFakeHolidayLeave();
        var fakeLeaveTypeToChange = fakeLeaveTypeFromDb.Clone()!;
        fakeLeaveTypeToChange.Name = "fake name";
        fakeLeaveTypeToChange.Order = 4;
        var mockSet = FakeLeaveTypeProvider.GetLeaveTypes().AsQueryable().BuildMockDbSet(); 
        mockSet.Setup(m => m.FindAsync(new object[] {holidayLeaveGuid}, default))
            .ReturnsAsync(fakeLeaveTypeToChange);

        var dbContextMock = new Mock<LeaveSystemDbContext>();
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<LeaveType>())
            .Returns(mockSet.Object);
        var internalEntityMock = new Mock<EntityEntry<LeaveType>>(null);
        internalEntityMock.Setup(x => x.State).Returns(EntityState.Unchanged);
        dbContextMock.Setup(m => m.Entry(fakeLeaveTypeToChange))
            .Returns(internalEntityMock.Object);

        var sut = new LeaveTypesController(dbContextMock.Object);
        //When
        var act = async () =>
        {
            await sut.Put(holidayLeaveGuid, fakeLeaveTypeToChange);
        };
        //Then
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task WhenModelIsValidAndSameProvidedIdAndLeaveRequestIdAndNoExceptionWasThrown_ThenReturnUpdated()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        var fakeLeaveTypes = FakeLeaveTypeProvider.GetLeaveTypes();
        foreach (var leaveType in fakeLeaveTypes)
        {
            await sut.Post(leaveType);
        }
        
        var fakeLeaveTypeToChange = sut.Get(FakeLeaveTypeProvider.FakeOnDemandLeaveId).Queryable.First();
        fakeLeaveTypeToChange.Name = "fake name";
        fakeLeaveTypeToChange.Order = 4;
        var updatedLeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId;
        //When
        var result = await sut.Put(updatedLeaveTypeId, fakeLeaveTypeToChange);
        //Then
        result.Should().BeOfType<UpdatedODataResult<LeaveType>>();
        sut.Get(updatedLeaveTypeId).Queryable.First().Should().BeEquivalentTo(new
            {
                Name = fakeLeaveTypeToChange.Name,
                Order = fakeLeaveTypeToChange.Order
            }, o => o.ExcludingMissingMembers()
        );
    }
}