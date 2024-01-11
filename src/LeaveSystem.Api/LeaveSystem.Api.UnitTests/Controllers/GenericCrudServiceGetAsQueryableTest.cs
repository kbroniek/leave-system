using FluentAssertions.Equivalency;
using FluentValidation;
using GoldenEye.Shared.Core.Objects.General;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.UnitTests.Providers;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GenericCrudServiceGetAsQueryableTest
{
    private GenericCrudService<TEntity, TId> GetSut<TEntity, TId>(LeaveSystemDbContext dbContext, DeltaValidator<TEntity> deltaValidator, IValidator<TEntity> entityValidator)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId> =>
        new (dbContext, deltaValidator, entityValidator);

    [Fact]
    public async Task WhenGettingByKey_ReturnEntityWithKey()
    {
        await WhenGettingByKey_ReturnEntityWithKey_Helper<UserLeaveLimit, Guid>(
            FakeUserLeaveLimitProvider.GetLimits(),
            FakeUserLeaveLimitProvider.FakeLimitForOnDemandLeaveId,
            FakeUserLeaveLimitProvider.GetLimitForOnDemandLeave()
            );
        await WhenGettingByKey_ReturnEntityWithKey_Helper<Setting, string>(
            FakeSettingsProvider.GetSettings(),
            FakeSettingsProvider.AcceptedSettingId,
            FakeSettingsProvider.GetAcceptedSetting(),
            JsonDocumentCompareOptionsProvider.Get<Setting>("Value")
            );
        await WhenGettingByKey_ReturnEntityWithKey_Helper<LeaveType, Guid>(
            FakeLeaveTypeProvider.GetLeaveTypes().ToList(),
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            FakeLeaveTypeProvider.GetFakeOnDemandLeave());
    }

    private async Task WhenGettingByKey_ReturnEntityWithKey_Helper<TEntity, TId>(
        IEnumerable<TEntity> entities,
        TId key,
        TEntity expectedEntity,
        Func<EquivalencyAssertionOptions<TEntity>, EquivalencyAssertionOptions<TEntity>>? config = null)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        var setMock = entities.AsQueryable().BuildMockDbSet();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        dbContextMock.Setup(m => m.Set<TEntity>()).Returns(setMock.Object);
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        var sut = GetSut<TEntity, TId>(dbContextMock.Object, deltaValidatorMock.Object, entityValidatorMock.Object);
        var result = sut.GetSingleAsQueryable(key);
        result.Count().Should().Be(1);
        if (config is not null)
        {
            result.First().Should().BeEquivalentTo(expectedEntity, config);
            return;
        }
        result.First().Should().BeEquivalentTo(expectedEntity);

    }
}
