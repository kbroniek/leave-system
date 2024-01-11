using FluentValidation;
using GoldenEye.Shared.Core.Objects.General;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using NSubstitute.ExceptionExtensions;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GenericCrudServiceAddAsyncTest
{
    private GenericCrudService<TEntity, TId> GetSut<TEntity, TId>(LeaveSystemDbContext dbContext, DeltaValidator<TEntity> deltaValidator, IValidator<TEntity> entityValidator)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId> =>
        new(dbContext, deltaValidator, entityValidator);

    [Fact]
    public async Task WhenEntityValidatorThrowsException_ThenNotAddEntity()
    {
        await WhenEntityValidatorThrowsException_ThenNotAddEntity_Helper<UserLeaveLimit, Guid>(new UserLeaveLimit());
        await WhenEntityValidatorThrowsException_ThenNotAddEntity_Helper<Setting, string>(new Setting());
        await WhenEntityValidatorThrowsException_ThenNotAddEntity_Helper<LeaveType, Guid>(new LeaveType());
    }
    private async Task WhenEntityValidatorThrowsException_ThenNotAddEntity_Helper<TEntity, TId>(TEntity entity)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        //Given
        var setMock = Enumerable.Empty<TEntity>().AsQueryable().BuildMockDbSet();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.Set<TEntity>()).Returns(setMock.Object);
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        entityValidatorMock.Setup(m =>
            m.ValidateAsync(It.IsAny<IValidationContext>(), new CancellationToken()))
            .ThrowsAsync(new Exception());
        var sut = GetSut<TEntity, TId>(dbContextMock.Object, deltaValidatorMock.Object, entityValidatorMock.Object);
        //When
        var act = async () =>
        {
            await sut.AddAsync(entity);
        };
        //Then
        await act.Should().ThrowAsync<Exception>();
        setMock.Verify(m => m.Add(entity), Times.Never);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenEntityValid_ThenSaveToDbAndReturnEntity()
    {
        await WhenEntityValid_ThenSaveToDbAndReturnEntity_Helper<UserLeaveLimit, Guid>(FakeUserLeaveLimitProvider.GetLimitForSickLeave());
        await WhenEntityValid_ThenSaveToDbAndReturnEntity_Helper<Setting, string>(FakeSettingsProvider.GetAcceptedSetting());
        await WhenEntityValid_ThenSaveToDbAndReturnEntity_Helper<LeaveType, Guid>(FakeLeaveTypeProvider.GetFakeSickLeave());
    }

    private async Task WhenEntityValid_ThenSaveToDbAndReturnEntity_Helper<TEntity, TId>(TEntity entity)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        var setMock = Enumerable.Empty<TEntity>().AsQueryable().BuildMockDbSet();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.Set<TEntity>()).Returns(setMock.Object);
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        var sut = GetSut<TEntity, TId>(dbContextMock.Object, deltaValidatorMock.Object, entityValidatorMock.Object);
        var result = await sut.AddAsync(entity);
        result.Should().BeEquivalentTo(entity);
        entityValidatorMock.Verify(m => m.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()), Times.Once);
        setMock.Verify(m => m.Add(entity), Times.Once);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
