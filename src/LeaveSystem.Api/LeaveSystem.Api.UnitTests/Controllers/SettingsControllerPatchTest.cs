using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class SettingsControllerPatchTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new SettingsController(dbContext);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        var fakeSettingId = FakeSettingsProvider.AcceptedSettingId;
        var fakeDelta = new Delta<Setting>();
        //When
        var result = await sut.Patch(fakeSettingId, fakeDelta);
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Theory]
    [MemberData(nameof(Get_Settings_TestData))]
    public async Task WhenNoLeaveRequestWithSuchId_ThenReturnNotFound(IEnumerable<Setting> fakeSettings)
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.AddRangeAsync(fakeSettings);
        await dbContext.SaveChangesAsync();
        var sut = new SettingsController(dbContext);
        var fakeDelta = new Delta<Setting>();
        //When
        var result = await sut.Patch(Guid.NewGuid().ToString(), fakeDelta);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }
    
    public static IEnumerable<object[]> Get_Settings_TestData()
    {
        yield return new object[] { Enumerable.Empty<Setting>() };
        yield return new object[] { FakeSettingsProvider.GetSettings() };
    }
    
    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndSettingNotExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var fakeSetting = FakeSettingsProvider.GetAcceptedSetting();
        var mockSet = new []
        {
            FakeSettingsProvider.GetCanceledSetting(),
            FakeSettingsProvider.GetRejectedSetting()
        }.AsQueryable().BuildMockDbSet(); 
        mockSet.Setup(m => m.FindAsync(new object[] {fakeSetting.Id}, default))
            .ReturnsAsync(fakeSetting);

        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<Setting>())
            .Returns(mockSet.Object);
        var fakeDelta = new Delta<Setting>();
        fakeDelta.TrySetPropertyValue(nameof(Setting.Value), JsonDocument.Parse("{\"color\": \"white\"}"));
        var sut = new SettingsController(dbContextMock.Object);
        //When
        var result = await sut.Patch(FakeSettingsProvider.AcceptedSettingId, fakeDelta);
        //Then
        result.Should().BeOfType<NotFoundResult>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndSettingExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var fakeSetting = FakeSettingsProvider.GetAcceptedSetting();
        var mockSet = FakeSettingsProvider.GetSettings().AsQueryable().BuildMockDbSet(); 
        mockSet.Setup(m => m.FindAsync(new object[] {fakeSetting.Id}, default))
            .ReturnsAsync(fakeSetting);

        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<Setting>())
            .Returns(mockSet.Object);
        var fakeDelta = new Delta<Setting>();
        fakeDelta.TrySetPropertyValue(nameof(Setting.Value), JsonDocument.Parse("{\"color\": \"white\"}"));
        var sut = new SettingsController(dbContextMock.Object);
        //When
        var act = async () =>
        {
            await sut.Patch(FakeSettingsProvider.AcceptedSettingId, fakeDelta);
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
        await dbContext.Settings.AddRangeAsync(FakeSettingsProvider.GetSettings());
        var sut = new SettingsController(dbContext);
        var fakeDelta = new Delta<Setting>();
        var updatedSettingValue = JsonDocument.Parse("{\"color\": \"white\"}");
        fakeDelta.TrySetPropertyValue(nameof(Setting.Value), updatedSettingValue);
        var updatedSettingId = FakeSettingsProvider.AcceptedSettingId;
        //When
        var result = await sut.Patch(updatedSettingId, fakeDelta);
        //Then
        result.Should().BeOfType<UpdatedODataResult<Setting>>();
        sut.Get(updatedSettingId).Queryable.First().Should().BeEquivalentTo(new
            {
                Name = updatedSettingValue
            }, o => o.ExcludingMissingMembers()
        );
    }
}