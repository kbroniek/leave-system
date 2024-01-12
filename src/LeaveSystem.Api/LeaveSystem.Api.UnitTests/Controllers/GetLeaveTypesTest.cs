using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GetLeaveTypesTest
{
    private LeaveTypesController GetSut(
        GenericCrudService<LeaveType, Guid> crudService) => new(crudService);

    [Fact]
    public void WhenGetting_ThenReturnData()
    {
        var crudServiceMock = GetCrudServiceMock();
        var fakeUserLimits = FakeLeaveTypeProvider.GetLeaveTypes();
        crudServiceMock.Setup(x => x.Get()).Returns(fakeUserLimits);
        var sut = GetSut(crudServiceMock.Object);
        var result = sut.Get();
        result.Should().BeEquivalentTo(fakeUserLimits);
        crudServiceMock.Verify(m => m.Get(), Times.Once);
    }

    private Mock<GenericCrudService<LeaveType, Guid>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<LeaveType>>().Object,
            new Mock<IValidator<LeaveType>>().Object);
}