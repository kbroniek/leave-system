using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GetUserLimitTest
{

    [Fact]
    public void WhenGetting_ThenReturnData()
    {
        var neighbouringLimitsServiceMock = GetNeighbouringLimitsServiceMock();
        var crudServiceMock = GetCrudServiceMock();
        var fakeId = FakeUserLeaveLimitProvider.FakeLimitForOnDemandLeaveId;
        crudServiceMock.Setup(x => x.GetSingleAsQueryable(fakeId))
            .Returns(new[]
            {
                FakeUserLeaveLimitProvider.GetLimitForOnDemandLeave()
            }.AsQueryable());
        var sut = new UserLeaveLimitsController(crudServiceMock.Object,  neighbouringLimitsServiceMock.Object);
        var result = sut.Get(fakeId);
        var expectedResult = SingleResult.Create(new[]
        {
            FakeUserLeaveLimitProvider.GetLimitForOnDemandLeave()
        }.AsQueryable());
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    private Mock<NeighbouringLimitsService> GetNeighbouringLimitsServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object);
    
    private Mock<GenericCrudService<UserLeaveLimit, Guid>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<UserLeaveLimit>>().Object,
            new Mock<IValidator<UserLeaveLimit>>().Object);
}