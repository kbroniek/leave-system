using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.UnitTests.Providers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GetSettingTest
{
    [Fact]
    public void WhenGetting_ThenReturnData()
    {
        var crudServiceMock = GetCrudServiceMock();
        var fakeId = FakeSettingsProvider.AcceptedSettingId;
        crudServiceMock.Setup(x => x.GetSingleAsQueryable(fakeId))
            .Returns(new[]
            {
                FakeSettingsProvider.GetAcceptedSetting()
            }.AsQueryable());
        var sut = new SettingsController(crudServiceMock.Object);
        var result = sut.Get(fakeId);
        var expectedResult = SingleResult.Create(new[]
        {
            FakeSettingsProvider.GetAcceptedSetting()
        }.AsQueryable());
        result.Should().BeEquivalentTo(expectedResult, JsonDocumentCompareOptionsProvider.Get<SingleResult<Setting>>("Value"));
    }
    
    private Mock<GenericCrudService<Setting, string>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<Setting>>().Object,
            new Mock<IValidator<Setting>>().Object);
}