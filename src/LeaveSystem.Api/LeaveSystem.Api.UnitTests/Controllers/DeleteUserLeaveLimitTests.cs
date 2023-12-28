using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class DeleteUserLeaveLimitTests
{
    [Fact]
    public async Task WhenDelete_ThenReturnNoContentResponse()
    {
        var neighbouringLimitsServiceMock = GetNeighbouringLimitsServiceMock();
        var crudServiceMock = GetCrudServiceMock();
        var sut = new UserLeaveLimitsController(crudServiceMock.Object, neighbouringLimitsServiceMock.Object);
        var fakeId = Guid.Parse("e8abc39b-81b3-4180-a864-85d225630e6b");
        var result = await sut.Delete(fakeId);
        result.Should().BeOfType<NoContentResult>();
        crudServiceMock.Verify(m => m.DeleteAsync(fakeId, It.IsAny<CancellationToken>()));
    }
    
    private static Mock<NeighbouringLimitsService> GetNeighbouringLimitsServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object);

    private static Mock<GenericCrudService<UserLeaveLimit, Guid>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<UserLeaveLimit>>().Object,
            new Mock<IValidator<UserLeaveLimit>>().Object);
}