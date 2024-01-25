using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GetSettingsTest
{
    private SettingsController GetSut(
        GenericCrudService<Setting, string> crudService) => new(crudService);

    [Fact]
    public void WhenGetting_ThenReturnData()
    {
        var crudServiceMock = GetCrudServiceMock();
        var fakeSettings = FakeSettingsProvider.GetSettings();
        crudServiceMock.Setup(x => x.Get()).Returns(fakeSettings);
        var sut = GetSut(crudServiceMock.Object);
        var result = sut.Get();
        result.Should().BeEquivalentTo(fakeSettings);
        crudServiceMock.Verify(m => m.Get(), Times.Once);
    }

    private Mock<GenericCrudService<Setting, string>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<Setting>>().Object,
            new Mock<IValidator<Setting>>().Object);
}