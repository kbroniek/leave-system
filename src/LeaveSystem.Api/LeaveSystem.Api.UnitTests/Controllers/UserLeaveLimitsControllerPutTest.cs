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
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class UserLeaveLimitsControllerPutTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new UserLeaveLimitsController(dbContext);
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
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        var fakeLimitId = FakeUserLeaveLimitProvider.FakeLimitForOnDemandLeaveId;
        var fakeLimit = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        //When
        var result = await sut.Put(fakeLimitId, fakeLimit);
        //Then
        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task WhenProvidedUserLeaveLimitNotExistsInDatabase_ThenReturnNotFound()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        var fakeLimitId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        var fakeLimit = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        //When
        var result = await sut.Put(fakeLimitId, fakeLimit);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndUserLeaveLimitExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var acceptedLimitId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        var fakeLimitFromDb = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        var fakeLimitToChange = fakeLimitFromDb.Clone()!;
        fakeLimitToChange.AssignedToUserId = Guid.NewGuid().ToString();
        fakeLimitToChange.Limit = TimeSpan.FromHours(16);
        fakeLimitToChange.ValidSince = DateTimeOffset.Now + TimeSpan.FromDays(2);
        fakeLimitToChange.ValidUntil = DateTimeOffset.Now + TimeSpan.FromDays(5);
        fakeLimitToChange.LeaveTypeId = FakeLeaveTypeProvider.FakeSickLeaveId;
        fakeLimitToChange.Property = new UserLeaveLimit.UserLeaveLimitProperties
        {
            Description = "fake desc"
        };
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.AddAsync(fakeLimitFromDb);
        await dbContext.SaveChangesAsync();

        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<UserLeaveLimit>())
            .Returns(dbContext.Set<UserLeaveLimit>());
        dbContextMock.Setup(m => m.Entry(fakeLimitFromDb))
            .Returns(dbContext.Entry(fakeLimitFromDb));
        dbContextMock.Setup(m => m.Entry(fakeLimitToChange))
            .Returns(dbContext.Entry(fakeLimitToChange));
        var sut = new UserLeaveLimitsController(dbContextMock.Object);
        //When
        var act = async () => { await sut.Put(acceptedLimitId, fakeLimitToChange); };
        //Then
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task WhenModelIsValidAndSameProvidedIdAndUserLeaveLimitIdAndNoExceptionWasThrown_ThenReturnUpdated()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.UserLeaveLimits.AddRangeAsync(FakeUserLeaveLimitProvider.GetLimits());
        await dbContext.SaveChangesAsync();
        var sut = new UserLeaveLimitsController(dbContext);
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
        var updatedLimitId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        //When
        var result = await sut.Put(updatedLimitId, fakeLimitToChange);
        //Then
        result.Should().BeOfType<UpdatedODataResult<UserLeaveLimit>>();
        sut.Get(updatedLimitId).Queryable.First().Should().BeEquivalentTo(new
            {
                fakeLimitToChange
            }, o => o.ExcludingMissingMembers()
        );
    }
}