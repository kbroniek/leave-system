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
    private UserLeaveLimitsController GetSut(LeaveSystemDbContext dbContext,
        GenericCrudService<UserLeaveLimit, Guid> crudService) => new(dbContext, crudService);

    [Fact]
    public void WhenGetting_ThenReturnData()
    {
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var crudServiceMock = GetCrudServiceMock();
        var fakeUserLimits = FakeUserLeaveLimitProvider.GetLimits();
        crudServiceMock.Setup(x => x.Get()).Returns(fakeUserLimits);
        var sut = GetSut(dbContextMock.Object, crudServiceMock.Object);
        var result = sut.Get();
        result.Should().BeEquivalentTo(fakeUserLimits);
    }

    private Mock<GenericCrudService<UserLeaveLimit, Guid>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<UserLeaveLimit>>().Object,
            new Mock<IValidator<UserLeaveLimit>>().Object);
}