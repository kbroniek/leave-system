using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GetLeaveTypeTest
{
    [Fact]
    public void WhenGetting_ThenReturnData()
    {
        var crudServiceMock = GetCrudServiceMock();
        var fakeId = FakeUserLeaveLimitProvider.FakeLimitForOnDemandLeaveId;
        crudServiceMock.Setup(x => x.GetSingleAsQueryable(fakeId))
            .Returns(new[]
            {
                FakeLeaveTypeProvider.GetFakeOnDemandLeave()
            }.AsQueryable());
        var sut = new LeaveTypesController(crudServiceMock.Object);
        var result = sut.Get(fakeId);
        var expectedResult = SingleResult.Create(new[]
        {
            FakeLeaveTypeProvider.GetFakeOnDemandLeave()
        }.AsQueryable());
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    private Mock<GenericCrudService<LeaveType, Guid>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<LeaveType>>().Object,
            new Mock<IValidator<LeaveType>>().Object);
}