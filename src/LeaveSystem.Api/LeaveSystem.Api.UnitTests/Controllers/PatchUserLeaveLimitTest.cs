using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.UnitTests.TestExtensions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared.UserLeaveLimits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class PatchUserLeaveLimitTest
{
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenThrowBadHttpException()
    {
        var neighbouringLimitsServiceMock = GetNeighbouringLimitsServiceMock();
        var crudServiceMock = GetCrudServiceMock();
        var delta = new Delta<UserLeaveLimit>();
        var sut = new UserLeaveLimitsController(crudServiceMock.Object, neighbouringLimitsServiceMock.Object);
        sut.ModelState.AddModelError("Error", "fake model state error");
        var act = async () => { await sut.Patch(Guid.Parse("e8abc39b-81b3-4180-a864-85d225630e6b"), delta); };
        await act.Should().ThrowAsync<BadHttpRequestException>();
    }

    [Fact]
    public async Task WhenEntityUpdated_ThenReturnWithCreatedStatusCode()
    {
        var neighbouringLimitsServiceMock = GetNeighbouringLimitsServiceMock();
        var crudServiceMock = GetCrudServiceMock();
        var fakeLimitId = Guid.Parse("59785ced-2587-4a45-bf63-e17dd353e5f0");
        var entity = new UserLeaveLimit
        {
            Id = fakeLimitId,
            AssignedToUserId = "0f203ba9-517c-4d1b-b756-32e7905abe73",
            LeaveTypeId = Guid.Parse("84221e7b-6bc3-4020-9f29-8aa3a51dbe6a"),
            Limit = TimeSpan.FromHours(12),
            OverdueLimit = TimeSpan.FromHours(4),
            Property = new()
            {
                Description = "fake desc"
            },
            ValidSince = DateTimeOffset.Parse("2023-12-21"),
            ValidUntil = DateTimeOffset.Parse("2023-12-15")
        };
        var delta = entity.ToDelta();
        crudServiceMock.Setup(m => m.PatchAsync(
                fakeLimitId,
                It.IsAny<Delta<UserLeaveLimit>>(),
                default))
            .ReturnsAsync(entity);
        var sut = new UserLeaveLimitsController(crudServiceMock.Object, neighbouringLimitsServiceMock.Object);
        var result = await sut.Patch(fakeLimitId, delta);
        result.Should().BeOfType<UpdatedODataResult<UserLeaveLimit>>();
        crudServiceMock.Verify(m => m.PatchAsync(fakeLimitId, It.IsAny<Delta<UserLeaveLimit>>(), It.IsAny<CancellationToken>()), Times.Once);
        neighbouringLimitsServiceMock.Verify(
            m => m.CloseNeighbourLimitsPeriodsAsync(It.Is<UserLeaveLimit>(ull => LimitAreEquivalents(ull, entity)), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private static bool LimitAreEquivalents(UserLeaveLimit firstLimit, UserLeaveLimit secondLimit) =>
        firstLimit.OverdueLimit == secondLimit.OverdueLimit &&
        firstLimit.Limit == secondLimit.Limit &&
        firstLimit.ValidSince == secondLimit.ValidSince &&
        firstLimit.ValidUntil == secondLimit.ValidUntil &&
        firstLimit.LeaveTypeId == secondLimit.LeaveTypeId &&
        firstLimit.AssignedToUserId == secondLimit.AssignedToUserId;

    private static Mock<NeighbouringLimitsService> GetNeighbouringLimitsServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object);

    private static Mock<GenericCrudService<UserLeaveLimit, Guid>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<UserLeaveLimit>>().Object,
            new Mock<IValidator<UserLeaveLimit>>().Object);
}