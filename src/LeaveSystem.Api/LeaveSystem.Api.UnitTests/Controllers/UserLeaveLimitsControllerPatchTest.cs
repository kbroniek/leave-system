using System.Text.Json;
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
using MockQueryable.Moq;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class UserLeaveLimitsControllerPatchTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        var fakeLimitId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        var fakeDelta = new Delta<UserLeaveLimit>();
        //When
        var result = await sut.Patch(fakeLimitId, fakeDelta);
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Theory]
    [MemberData(nameof(Get_UserLeaveLimits_TestData))]
    public async Task WhenNoUserLeaveLimitWithSuchId_ThenReturnNotFound(IEnumerable<Setting> fakeSettings)
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.AddRangeAsync(fakeSettings);
        await dbContext.SaveChangesAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        var fakeDelta = new Delta<UserLeaveLimit>();
        //When
        var result = await sut.Patch(Guid.NewGuid(), fakeDelta);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }
    
    public static IEnumerable<object[]> Get_UserLeaveLimits_TestData()
    {
        yield return new object[] { Enumerable.Empty<Setting>() };
        yield return new object[] { FakeSettingsProvider.GetSettings() };
    }
    
    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndUserLeaveLimitNotExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var fakeLimit = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        var mockSet = new []
        {
            FakeUserLeaveLimitProvider.GetLimitForSickLeave(),
            FakeUserLeaveLimitProvider.GetLimitForOnDemandLeave()
        }.AsQueryable().BuildMockDbSet(); 
        mockSet.Setup(m => m.FindAsync(new object[] {fakeLimit.Id}, default))
            .ReturnsAsync(fakeLimit);

        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<UserLeaveLimit>())
            .Returns(mockSet.Object);
        var fakeDelta = new Delta<UserLeaveLimit>();
        fakeDelta.TrySetPropertyValue(nameof(UserLeaveLimit.AssignedToUserId), Guid.NewGuid().ToString());
        var sut = new UserLeaveLimitsController(dbContextMock.Object);
        //When
        var result = await sut.Patch(FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId, fakeDelta);
        //Then
        result.Should().BeOfType<NotFoundResult>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndUserLeaveLimitExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var fakeLimit = FakeUserLeaveLimitProvider.GetLimitForHolidayLeave();
        var mockSet = FakeUserLeaveLimitProvider.GetLimits().AsQueryable().BuildMockDbSet(); 
        mockSet.Setup(m => m.FindAsync(new object[] {fakeLimit.Id}, default))
            .ReturnsAsync(fakeLimit);

        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<UserLeaveLimit>())
            .Returns(mockSet.Object);
        var fakeDelta = new Delta<UserLeaveLimit>();
        fakeDelta.TrySetPropertyValue(nameof(UserLeaveLimit.AssignedToUserId), Guid.NewGuid().ToString());
        var sut = new UserLeaveLimitsController(dbContextMock.Object);
        //When
        var act = async () =>
        {
            await sut.Patch(FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId, fakeDelta);
        };
        //Then
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task WhenModelIsValidAndUserLeaveLimitExistsAndNoExceptionWasThrown_ThenUpdateEntitySuccessful()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.UserLeaveLimits.AddRangeAsync(FakeUserLeaveLimitProvider.GetLimits());
        await dbContext.LeaveTypes.AddAsync(FakeLeaveTypeProvider.GetFakeSickLeave());
        await dbContext.SaveChangesAsync();
        var sut = new UserLeaveLimitsController(dbContext);
        var fakePropsToUpdate = new
        {
            AssignedToUserId = Guid.NewGuid().ToString(),
            Limit = TimeSpan.FromHours(16),
            ValidSince = DateTimeOffset.Now + TimeSpan.FromDays(2),
            ValidUntil = DateTimeOffset.Now + TimeSpan.FromDays(5),
            LeaveTypeId = FakeLeaveTypeProvider.FakeSickLeaveId,
            Property = new UserLeaveLimit.UserLeaveLimitProperties
            {
                Description = "fake desc"
            }
        };
        var fakeDelta = fakePropsToUpdate.ToDelta<UserLeaveLimit>();
        var updatedLeaveTypeId = FakeUserLeaveLimitProvider.FakeLimitForHolidayLeaveId;
        //When
        var result = await sut.Patch(updatedLeaveTypeId, fakeDelta);
        //Then
        result.Should().BeOfType<UpdatedODataResult<UserLeaveLimit>>();
        sut.Get(updatedLeaveTypeId).Queryable.First().Should().BeEquivalentTo(
            fakePropsToUpdate, o => o.ExcludingMissingMembers()
        );
    }
}