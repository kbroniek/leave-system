using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.UnitTests.TestExtensions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MockQueryable.Moq;
using Moq;
using System.Runtime.Serialization;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class LeaveTypesControllerPutTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var sut = new LeaveTypesController(dbContextMock.Object);
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
        var fakeLeaveTypeId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid;
        var fakeLeaveType = FakeLeaveTypeProvider.GetFakeOnDemandLeave();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var leaveTypeEntityEntryMock = new Mock<EntityEntry<LeaveType>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
        dbContextMock.Setup(m => m.Entry(fakeLeaveType))
            .Returns(leaveTypeEntityEntryMock.Object);
        var sut = new LeaveTypesController(dbContextMock.Object);
        //When
        var result = await sut.Put(fakeLeaveTypeId, fakeLeaveType);
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task WhenProvidedLeaveTypeNotExistsInDatabase_ThenReturnNotFound()
    {
        //Given
        var fakeLeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId;
        var fakeLeaveType = FakeLeaveTypeProvider.GetFakeOnDemandLeave();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var leaveTypeEntityEntryMock = new Mock<EntityEntry<LeaveType>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));

        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Entry(fakeLeaveType))
            .Returns(leaveTypeEntityEntryMock.Object);
        dbContextMock.Setup(x => x.Set<LeaveType>()).Returns(FakeLeaveTypeProvider.GetLeaveTypes().Skip(2).BuildMockDbSet().Object);
        var sut = new LeaveTypesController(dbContextMock.Object);
        //When
        var result = await sut.Put(fakeLeaveTypeId, fakeLeaveType);
        //Then
        result.Should().BeOfType<NotFoundObjectResult>();
        leaveTypeEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
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
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var leaveTypeEntityEntryMock = new Mock<EntityEntry<LeaveType>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));

        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Entry(fakeLeaveTypeToChange))
            .Returns(leaveTypeEntityEntryMock.Object);
        dbContextMock.Setup(x => x.Set<LeaveType>()).Returns(FakeLeaveTypeProvider.GetLeaveTypes().BuildMockDbSet().Object);
        var sut = new LeaveTypesController(dbContextMock.Object);
        //When
        var act = async () =>
        {
            await sut.Put(holidayLeaveGuid, fakeLeaveTypeToChange);
        };
        //Then
        await act.Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
        leaveTypeEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
    }

    [Fact]
    public async Task WhenModelIsValidAndSameProvidedIdAndLeaveRequestIdAndNoExceptionWasThrown_ThenReturnUpdated()
    {
        //Given
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());

        var fakeLeaveTypeToChange = new LeaveType
        {
            Id = FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            Name = "fake name",
            Order = 4,
        };
        var leaveTypeEntityEntryMock = new Mock<EntityEntry<LeaveType>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
        dbContextMock.Setup(m => m.Entry(fakeLeaveTypeToChange))
            .Returns(leaveTypeEntityEntryMock.Object);
        var updatedLeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId;
        dbContextMock.Setup(x => x.Set<LeaveType>()).Returns(FakeLeaveTypeProvider.GetLeaveTypes().BuildMockDbSet().Object);
        var sut = new LeaveTypesController(dbContextMock.Object);
        //When
        var result = await sut.Put(updatedLeaveTypeId, fakeLeaveTypeToChange);
        //Then
        result.Should().BeOfType<UpdatedODataResult<LeaveType>>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
        leaveTypeEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
    }
}