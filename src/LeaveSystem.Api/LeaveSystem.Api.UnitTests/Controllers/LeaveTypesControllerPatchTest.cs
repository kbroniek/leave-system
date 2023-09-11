using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using DbUpdateConcurrencyException = Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class LeaveTypesControllerPatchTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        var fakeLeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId;
        var fakeDelta = new Delta<LeaveType>();
        //When
        var result = await sut.Patch(fakeLeaveTypeId, fakeDelta);
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [MemberData(nameof(Get_LeveRequests_TestData))]
    public async Task WhenNoLeaveRequestWithSuchId_ThenReturnNotFound(IEnumerable<LeaveType> fakeLeaveTypes)
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        var fakeDelta = new Delta<LeaveType>();
        foreach (var leaveType in fakeLeaveTypes)
        {
            await sut.Post(leaveType);
        }
        //When
        var result = await sut.Patch(Guid.NewGuid(), fakeDelta);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }

    public static IEnumerable<object[]> Get_LeveRequests_TestData()
    {
        yield return new object[] { Enumerable.Empty<LeaveType>() };
        yield return new object[] { FakeLeaveTypeProvider.GetLeaveTypes() };
    }

    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndProductNotExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var fakeLeave = FakeLeaveTypeProvider.GetFakeHolidayLeave();
        var mockSet = new[]
        {
            FakeLeaveTypeProvider.GetFakeSickLeave(),
            FakeLeaveTypeProvider.GetFakeOnDemandLeave()
        }.AsQueryable().BuildMockDbSet();
        mockSet.Setup(m => m.FindAsync(new object[] { fakeLeave.Id }, default))
            .ReturnsAsync(fakeLeave);

        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<LeaveType>())
            .Returns(mockSet.Object);
        var fakeDelta = new Delta<LeaveType>();
        fakeDelta.TrySetPropertyValue(nameof(LeaveType.Name), "fake changed name");
        var sut = new LeaveTypesController(dbContextMock.Object);
        //When
        var result = await sut.Patch(FakeLeaveTypeProvider.FakeHolidayLeaveGuid, fakeDelta);
        //Then
        result.Should().BeOfType<NotFoundResult>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndProductExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var fakeLeave = FakeLeaveTypeProvider.GetFakeHolidayLeave();
        var mockSet = FakeLeaveTypeProvider.GetLeaveTypes().AsQueryable().BuildMockDbSet();
        mockSet.Setup(m => m.FindAsync(new object[] { fakeLeave.Id }, default))
            .ReturnsAsync(fakeLeave);

        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<LeaveType>())
            .Returns(mockSet.Object);
        var fakeDelta = new Delta<LeaveType>();
        fakeDelta.TrySetPropertyValue(nameof(LeaveType.Name), "fake changed name");
        var sut = new LeaveTypesController(dbContextMock.Object);
        //When
        var act = async () =>
        {
            await sut.Patch(FakeLeaveTypeProvider.FakeHolidayLeaveGuid, fakeDelta);
        };
        //Then
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenModelIsValidAndLeaveRequestExistsAndNoExceptionWasThrown_ThenUpdateEntitySuccessful()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new LeaveTypesController(dbContext);
        var fakeLeaveTypes = FakeLeaveTypeProvider.GetLeaveTypes();
        foreach (var leaveType in fakeLeaveTypes)
        {
            await sut.Post(leaveType);
        }
        var fakeDelta = new Delta<LeaveType>();
        var updatedLeaveTypeName = "fake updated name";
        fakeDelta.TrySetPropertyValue(nameof(LeaveType.Name), updatedLeaveTypeName);
        var updatedLeaveTypeId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid;
        //When
        var result = await sut.Patch(updatedLeaveTypeId, fakeDelta);
        //Then
        result.Should().BeOfType<UpdatedODataResult<LeaveType>>();
        sut.Get(updatedLeaveTypeId).Queryable.First().Should().BeEquivalentTo(new
        {
            Name = updatedLeaveTypeName
        }, o => o.ExcludingMissingMembers()
        );
    }
}