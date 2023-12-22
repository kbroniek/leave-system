using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GetUserLimitsTest
{
    private UserLeaveLimitsController GetSut(
        GenericCrudService<UserLeaveLimit, Guid> crudService,
        NeighbouringLimitsService neighbouringLimitsService) => new(crudService, neighbouringLimitsService);

    [Fact]
    public void WhenGetting_ThenReturnData()
    {
        var limitsServiceMock = GetNeighbouringLimitsServiceMock();
        var crudServiceMock = GetCrudServiceMock();
        var fakeUserLimits = FakeUserLeaveLimitProvider.GetLimits();
        crudServiceMock.Setup(x => x.Get()).Returns(fakeUserLimits);
        var sut = GetSut(crudServiceMock.Object, limitsServiceMock.Object);
        var result = sut.Get();
        result.Should().BeEquivalentTo(fakeUserLimits);
    }
    
    private Mock<NeighbouringLimitsService> GetNeighbouringLimitsServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object);

    private Mock<GenericCrudService<UserLeaveLimit, Guid>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<UserLeaveLimit>>().Object,
            new Mock<IValidator<UserLeaveLimit>>().Object);
}