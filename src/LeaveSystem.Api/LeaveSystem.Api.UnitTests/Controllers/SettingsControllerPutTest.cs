using System.Runtime.Serialization;
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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class SettingsControllerPutTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenReturnBadRequest()
    {
        //Given
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var sut = new SettingsController(dbContextMock.Object);
        sut.ModelState.AddModelError("fakeKey", "fake error message");
        var fakeSettingId = FakeSettingsProvider.AcceptedSettingId;
        var fakeSetting = FakeSettingsProvider.GetAcceptedSetting();
        //When
        var result = await sut.Put(fakeSettingId, fakeSetting);
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async Task WhenProvidedIdIsDifferentThanEntityId_ThenReturnBadRequest()
    {
        //Given
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var sut = new SettingsController(dbContextMock.Object);
        var fakeSettingId = FakeSettingsProvider.AcceptedSettingId;
        var fakeSetting = FakeSettingsProvider.GetCanceledSetting();
        var settingEntityEntryMock = new Mock<EntityEntry<LeaveType>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
        
        //When
        var result = await sut.Put(fakeSettingId, fakeSetting);
        //Then
        result.Should().BeOfType<BadRequestResult>();
    }
    
    [Fact]
    public async Task WhenProvidedSettingNotExistsInDatabase_ThenReturnNotFound()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = new SettingsController(dbContext);
        var fakeSettingId = FakeSettingsProvider.AcceptedSettingId;
        var fakeSetting = FakeSettingsProvider.GetAcceptedSetting();
        //When
        var result = await sut.Put(fakeSettingId, fakeSetting);
        //Then
        result.Should().BeOfType<NotFoundResult>();
    }
    
    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndSettingExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var acceptedSettingId = FakeSettingsProvider.AcceptedSettingId;
        var fakeSettingFromDb = FakeSettingsProvider.GetAcceptedSetting();
        var fakeSettingToChange = fakeSettingFromDb.Clone()!;
        fakeSettingToChange.Value = JsonDocument.Parse("{\"fake\": \"fakeJsonValue\"}");
        
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.AddAsync(fakeSettingFromDb);
        await dbContext.SaveChangesAsync();

        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Set<Setting>())
            .Returns(dbContext.Set<Setting>());
        dbContextMock.Setup(m => m.Entry(fakeSettingFromDb))
            .Returns(dbContext.Entry(fakeSettingFromDb));
        dbContextMock.Setup(m => m.Entry(fakeSettingToChange))
            .Returns(dbContext.Entry(fakeSettingToChange));
        var sut = new SettingsController(dbContextMock.Object);
        //When
        var act = async () =>
        {
            await sut.Put(acceptedSettingId, fakeSettingToChange);
        };
        //Then
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task WhenModelIsValidAndSameProvidedIdAndSettingIdAndNoExceptionWasThrown_ThenReturnUpdated()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.AddRangeAsync(FakeSettingsProvider.GetSettings());
        await dbContext.SaveChangesAsync();
        var sut = new SettingsController(dbContext);
        var fakeSettingToChange = FakeSettingsProvider.GetAcceptedSetting();
        fakeSettingToChange.Value = JsonDocument.Parse("{\"fake\": \"fakeJsonValue\"}");
        var updatedSettingId = FakeSettingsProvider.AcceptedSettingId;
        //When
        var result = await sut.Put(updatedSettingId, fakeSettingToChange);
        //Then
        result.Should().BeOfType<UpdatedODataResult<Setting>>();
        sut.Get(updatedSettingId).Queryable.First().Should().BeEquivalentTo(new
            {
                Value = fakeSettingToChange.Value,
            }, o => o.ExcludingMissingMembers().ComparingByMembers<JsonElement>()
        );
    }
}