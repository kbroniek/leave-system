using System.Runtime.Serialization;
using System.Text.Json;
using FluentAssertions;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.UnitTests.TestExtensions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MockQueryable.Moq;
using Moq;
using NSubstitute;

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
        var result = await sut.PutAsync(fakeSettingId, fakeSetting);
        //Then
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    [Fact]
    public async Task WhenProvidedIdIsDifferentThanEntityId_ThenReturnBadRequest()
    {
        //Given
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var fakeSettingId = FakeSettingsProvider.AcceptedSettingId;
        var fakeSetting = FakeSettingsProvider.GetCanceledSetting();
        var settingEntityEntryMock = new Mock<EntityEntry<Setting>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
                dbContextMock.Setup(m => m.Entry(fakeSetting))
                    .Returns(settingEntityEntryMock.Object); 
        var sut = new SettingsController(dbContextMock.Object);
        //When
        var result = await sut.PutAsync(fakeSettingId, fakeSetting);
        //Then
        result.Should().BeOfType<BadRequestResult>();
    }
    
    [Fact]
    public async Task WhenProvidedSettingNotExistsInDatabase_ThenReturnNotFound()
    {
        //Given
        var fakeSettingId = FakeSettingsProvider.AcceptedSettingId;
        var fakeSetting = FakeSettingsProvider.GetAcceptedSetting();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var settingEntityEntryMock = new Mock<EntityEntry<Setting>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
        
        dbContextMock.Setup(m => m.Entry(fakeSetting))
            .Returns(settingEntityEntryMock.Object);
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(x => x.Set<Setting>()).Returns(FakeSettingsProvider.GetSettings().Skip(3).BuildMockDbSet().Object);
        var sut = new SettingsController(dbContextMock.Object);
        //When
        var result = await sut.PutAsync(fakeSettingId, fakeSetting);
        //Then
        result.Should().BeOfType<NotFoundResult>();
        settingEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
    }
    
    [Fact]
    public async Task WhenExceptionWasThrownDuringSavingChangesAndSettingExists_ThenThrowDbUpdateConcurrencyException()
    {
        //Given
        var acceptedSettingId = FakeSettingsProvider.AcceptedSettingId;
        var fakeSettingFromDb = FakeSettingsProvider.GetAcceptedSetting();
        var fakeSettingToChange = fakeSettingFromDb.Clone()!;
        fakeSettingToChange.Value = JsonDocument.Parse("{\"fake\": \"fakeJsonValue\"}");
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var settingEntityEntryMock = EntityEntryMockFactory.Create<Setting>();
        
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        dbContextMock.Setup(m => m.Entry(fakeSettingToChange))
            .Returns(settingEntityEntryMock.Object);
        dbContextMock.Setup(m => m.Set<Setting>())
            .Returns(FakeSettingsProvider.GetSettings().BuildMockDbSet().Object);
        var sut = new SettingsController(dbContextMock.Object);
        //When
        var act = async () =>
        {
            await sut.PutAsync(acceptedSettingId, fakeSettingToChange);
        };
        //Then
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
        settingEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
    }
    
    [Fact]
    public async Task WhenModelIsValidAndSameProvidedIdAndSettingIdAndNoExceptionWasThrown_ThenReturnUpdated()
    {
        //Given
        var fakeSettingToChange = new Setting()
        {
            Value = JsonDocument.Parse("{\"fake\": \"fakeJsonValue\"}"),
            Id = FakeSettingsProvider.AcceptedSettingId,
            Category = SettingCategoryType.LeaveStatus
        };
        var updatedSettingId = FakeSettingsProvider.AcceptedSettingId;
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var settingEntityEntryMock = EntityEntryMockFactory.Create<Setting>();
        
        dbContextMock.Setup(m => m.Entry(fakeSettingToChange))
            .Returns(settingEntityEntryMock.Object);
        dbContextMock.Setup(m => m.Set<Setting>())
            .Returns(FakeSettingsProvider.GetSettings().BuildMockDbSet().Object);
        var sut = new SettingsController(dbContextMock.Object);
        //When
        var result = await sut.PutAsync(updatedSettingId, fakeSettingToChange);
        //Then
        result.Should().BeOfType<UpdatedODataResult<Setting>>();
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()));
        settingEntityEntryMock.VerifySet(m => m.State = EntityState.Modified);
    }
}