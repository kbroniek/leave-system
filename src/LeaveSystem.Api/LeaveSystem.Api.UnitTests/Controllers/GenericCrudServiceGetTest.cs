using FluentValidation;
using GoldenEye.Shared.Core.Objects.General;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GenericCrudServiceGetTest
{

    private GenericCrudService<TEntity, TId> GetSut<TEntity, TId>(LeaveSystemDbContext dbContext, DeltaValidator<TEntity> deltaValidator, IValidator<TEntity> entityValidator)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId> =>
        new(dbContext, deltaValidator, entityValidator);

    [Fact]
    public void WhenGetting_ThenReturnAllEntitiesFromSet()
    {
        WhenGetting_ThenReturnAllEntitiesFromSet_Helper<UserLeaveLimit, Guid>(FakeUserLeaveLimitProvider.GetLimits().ToList());
        WhenGetting_ThenReturnAllEntitiesFromSet_Helper<LeaveType, Guid>(FakeLeaveTypeProvider.GetLeaveTypes().ToList());
        WhenGetting_ThenReturnAllEntitiesFromSet_Helper<Setting, string>(FakeSettingsProvider.GetSettings().ToList());
    }

    private void WhenGetting_ThenReturnAllEntitiesFromSet_Helper<TEntity, TId>(List<TEntity> dataFromSet)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        //Given
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var setMock = dataFromSet.AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(x => x.Set<TEntity>()).Returns(setMock.Object);
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        var sut = GetSut<TEntity, TId>(dbContextMock.Object, deltaValidatorMock.Object, entityValidatorMock.Object);
        //When
        var result = sut.Get();
        //Then
        result.Should().BeEquivalentTo(dataFromSet);
    }


}
