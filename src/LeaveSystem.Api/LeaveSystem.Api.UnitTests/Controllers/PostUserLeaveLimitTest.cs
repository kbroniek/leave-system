using FluentValidation;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared.Date;
using LeaveSystem.Shared.UserLeaveLimits;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class PostUserLeaveLimitTest
{
    private readonly CurrentDateService currentDateService = FakeDateServiceProvider.GetDateService();
        
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
        var now = currentDateService.GetWithoutTime();
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
            ValidSince = DateTimeOffset.Parse("2023-12-21 18:29:38 UTC")
        };
        var sut = new UserLeaveLimitsController(crudServiceMock.Object, neighbouringLimitsServiceMock.Object);
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
    
    private Mock<NeighbouringLimitsService> GetNeighbouringLimitsServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object);
    
    private Mock<GenericCrudService<UserLeaveLimit, Guid>> GetCrudServiceMock() =>
        new(new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>()).Object,
            new Mock<DeltaValidator<UserLeaveLimit>>().Object,
            new Mock<IValidator<UserLeaveLimit>>().Object);
}