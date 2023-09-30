using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.UnitTests.TestExtensions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NSubstitute;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class UserLeaveLimitsControllerPutTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var sut = new UserLeaveLimitsController(dbContextMock.Object);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        var fakeLimitId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        var fakeLimit = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        //When
        var result = await sut.Put(fakeLimitId, fakeLimit);
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task WhenProvidedIdIsDifferentThanEntityId_ThenReturnBadRequest()
    {
        //Given
        var fakeLimitId = FakeUserLeaveLimitProvider.FakeLimitForOnDemandLeaveId;
        var fakeLimit = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var userLeaveLimitEntityEntryMock = EntityEntryMockFactory.Create<UserLeaveLimit>();
        dbContextMock.Setup(m => m.Entry(fakeLimit))
            .Returns(userLeaveLimitEntityEntryMock.Object);
        var sut = new UserLeaveLimitsController(dbContextMock.Object);
        //When
        var result = await sut.Put(fakeLimitId, fakeLimit);
        //Then
        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task WhenProvidedUserLeaveLimitNotExistsInDatabase_ThenReturnNotFound()
    {
        //Given
        var fakeLimitId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        var fakeLimit = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var userLeaveLimitEntityEntryMock = EntityEntryMockFactory.Create<UserLeaveLimit>();
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Entry(fakeLimit))
            .Returns(userLeaveLimitEntityEntryMock.Object);
        dbContextMock.Setup(x => x.Set<UserLeaveLimit>()).Returns(FakeUserLeaveLimitProvider.GetLimits().Skip(2).BuildMockDbSet().Object);

        var sut = new UserLeaveLimitsController(dbContextMock.Object);
        //When
        var result = await sut.Put(fakeLimitId, fakeLimit);
        //Then
        result.Should().BeOfType<NotFoundResult>();
        userLeaveLimitEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
    }

    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndUserLeaveLimitExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var acceptedLimitId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        var fakeLimitToChange = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        fakeLimitToChange.AssignedToUserId = Guid.NewGuid().ToString();
        fakeLimitToChange.Limit = TimeSpan.FromHours(16);
        fakeLimitToChange.ValidSince = DateTimeOffset.Now + TimeSpan.FromDays(2);
        fakeLimitToChange.ValidUntil = DateTimeOffset.Now + TimeSpan.FromDays(5);
        fakeLimitToChange.LeaveTypeId = FakeLeaveTypeProvider.FakeSickLeaveId;
        fakeLimitToChange.Property = new UserLeaveLimit.UserLeaveLimitProperties
        {
            Description = "fake desc"
        };
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var userLeaveLimitEntityEntryMock = EntityEntryMockFactory.Create<UserLeaveLimit>();
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Entry(fakeLimitToChange))
            .Returns(userLeaveLimitEntityEntryMock.Object);
        dbContextMock.Setup(x => x.Set<UserLeaveLimit>()).Returns(FakeUserLeaveLimitProvider.GetLimits().BuildMockDbSet().Object);

        var sut = new UserLeaveLimitsController(dbContextMock.Object);
        //When
        var act = async () => { await sut.Put(acceptedLimitId, fakeLimitToChange); };
        //Then
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
        userLeaveLimitEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
    }
    
    [Fact]
    public async Task WhenModelIsValidAndSameProvidedIdAndUserLeaveLimitIdAndNoExceptionWasThrown_ThenReturnUpdated()
    {
        //Given
        var fakeLimitToChange = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        fakeLimitToChange.AssignedToUserId = Guid.NewGuid().ToString();
        fakeLimitToChange.Limit = TimeSpan.FromHours(16);
        fakeLimitToChange.ValidSince = DateTimeOffset.Now + TimeSpan.FromDays(2);
        fakeLimitToChange.ValidUntil = DateTimeOffset.Now + TimeSpan.FromDays(5);
        fakeLimitToChange.LeaveTypeId = FakeLeaveTypeProvider.FakeSickLeaveId;
        fakeLimitToChange.Property = new UserLeaveLimit.UserLeaveLimitProperties
        {
            Description = "fake desc"
        };
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var userLeaveLimitEntityEntryMock = EntityEntryMockFactory.Create<UserLeaveLimit>();
        dbContextMock.Setup(m => m.Entry(fakeLimitToChange))
            .Returns(userLeaveLimitEntityEntryMock.Object);
        dbContextMock.Setup(x => x.Set<UserLeaveLimit>()).Returns(FakeUserLeaveLimitProvider.GetLimits().BuildMockDbSet().Object);
        var sut = new UserLeaveLimitsController(dbContextMock.Object);
        var updatedLimitId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        //When
        var result = await sut.Put(updatedLimitId, fakeLimitToChange);
        //Then
        result.Should().BeOfType<UpdatedODataResult<UserLeaveLimit>>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
        userLeaveLimitEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
    }
}