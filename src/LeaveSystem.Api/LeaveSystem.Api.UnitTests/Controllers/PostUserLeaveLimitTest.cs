using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared.Date;
using LeaveSystem.Shared.UserLeaveLimits;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class PostUserLeaveLimitTest
{
    
    [Fact]
    public async Task WhenModelStateIsNotValid_ThenThrowBadHttpRequestException()
    {
        var neighbouringLimitsServiceMock = GetNeighbouringLimitsServiceMock();
        var crudServiceMock = GetCrudServiceMock();
        var postDto = new AddUserLeaveLimitDto();
        var sut = new UserLeaveLimitsController(crudServiceMock.Object, neighbouringLimitsServiceMock.Object);
        sut.ModelState.AddModelError("Error", "fake model state error");
        var act = async () =>
        {
            await sut.Post(postDto);
        };
        await act.Should().ThrowAsync<BadHttpRequestException>();
        crudServiceMock.Verify(
            m => m.AddAsync(It.IsAny<UserLeaveLimit>(), It.IsAny<CancellationToken>()), Times.Never);
        neighbouringLimitsServiceMock.Verify(m => 
            m.CloseNeighbourLimitsPeriodsAsync(It.IsAny<UserLeaveLimit>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenEntityAdded_ReturnWithCreatedStatusCode()
    {
        var neighbouringLimitsServiceMock = GetNeighbouringLimitsServiceMock();
        var crudServiceMock = GetCrudServiceMock();
        var postDto = new AddUserLeaveLimitDto()
        {
            AssignedToUserId = "0f203ba9-517c-4d1b-b756-32e7905abe73",
            LeaveTypeId = Guid.Parse("84221e7b-6bc3-4020-9f29-8aa3a51dbe6a"),
            Limit = TimeSpan.FromHours(12),
            OverdueLimit = TimeSpan.FromHours(4),
            Property = new AddUserLeaveLimitPropertiesDto
            {
                Description = "fake desc"
            },
            ValidSince = DateTimeOffset.Parse("2023-12-21"),
            ValidUntil = DateTimeOffset.Parse("2023-12-15")
        };
        var entityFromDto = new UserLeaveLimit
        {
            Id = Guid.NewGuid(),
            OverdueLimit = postDto.OverdueLimit,
            Limit = postDto.Limit,
            AssignedToUserId = postDto.AssignedToUserId,
            ValidSince = postDto.ValidSince,
            ValidUntil = postDto.ValidUntil,
            LeaveTypeId = postDto.LeaveTypeId,
            Property = new()
            {
                Description = postDto.Property?.Description
            }
        };
        crudServiceMock.Setup(m => m.AddAsync(
                It.Is<UserLeaveLimit>(ull => LimitAreEquivalents(ull, entityFromDto)),
                default))
            .ReturnsAsync(entityFromDto);
        var sut = new UserLeaveLimitsController(crudServiceMock.Object, neighbouringLimitsServiceMock.Object);
        var result = await sut.Post(postDto);
        result.Should().BeOfType<CreatedODataResult<UserLeaveLimit>>();
        crudServiceMock.Verify(
            m => m.AddAsync(It.IsAny<UserLeaveLimit>(), It.IsAny<CancellationToken>()), Times.Once);
        neighbouringLimitsServiceMock.Verify(m => 
            m.CloseNeighbourLimitsPeriodsAsync(It.Is<UserLeaveLimit>(ull => LimitAreEquivalents(ull, entityFromDto)), It.IsAny<CancellationToken>()), Times.Once);
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