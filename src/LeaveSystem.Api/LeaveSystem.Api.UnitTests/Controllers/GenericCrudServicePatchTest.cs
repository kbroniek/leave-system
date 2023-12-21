using System.Runtime.Serialization;
using FluentValidation;
using GoldenEye.Objects.General;
using LeaveSystem.Api.Controllers;
using LeaveSystem.Api.Endpoints.Errors;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.UnitTests.Providers;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MockQueryable.Moq;
using Moq;
using NSubstitute;

namespace LeaveSystem.Api.UnitTests.Controllers;

public class GenericCrudServicePatchTest
{
    private GenericCrudService<TEntity, TId> GetSut<TEntity, TId>(LeaveSystemDbContext dbContext, DeltaValidator<TEntity> deltaValidator, IValidator<TEntity> entityValidator) 
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId> =>
        new (dbContext, deltaValidator, entityValidator);

    [Fact]
    public async Task WhenEntityNotExistsThrow_EntityNotFoundException()
    {
        await WhenEntityNotExistsThrow_EntityNotFoundException_Helper<UserLeaveLimit, Guid>(Guid.Parse("8bd6f2d3-1a45-40ea-9bf6-e7805de52f1b"));
        await WhenEntityNotExistsThrow_EntityNotFoundException_Helper<LeaveType, Guid>(Guid.Parse("0520a2ff-7225-409a-8612-d2ef939e5eaf"));
        await WhenEntityNotExistsThrow_EntityNotFoundException_Helper<Setting, string>("65f7054a-25a5-4d4e-abba-4099a8c5dc31");
    }

    private async Task WhenEntityNotExistsThrow_EntityNotFoundException_Helper<TEntity, TId>(
        TId key)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var setMock = Enumerable.Empty<TEntity>().AsQueryable().BuildMockDbSet();
        dbContextMock.Setup(x => x.Set<TEntity>()).Returns(setMock.Object);
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        var sut = GetSut<TEntity, TId>(dbContextMock.Object,deltaValidatorMock.Object, entityValidatorMock.Object);
        var act = async () =>
        {
            await sut.PatchAsync(key, new Delta<TEntity>());
        };
        await act.Should().ThrowAsync<EntityNotFoundException>();
        setMock.Verify(m => m.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()), Times.Once);
        deltaValidatorMock.Verify(m => m.CreateDeltaWithoutProtectedProperties(It.IsAny<Delta<TEntity>>()), Times.Never);
        entityValidatorMock.Verify(m => m.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()), Times.Never);
        dbContextMock.Verify(m => m.Entry(It.IsAny<TEntity>()), Times.Never);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenEntityValidatorThrowException_ThenNotModifyEntity()
    {
        await WhenEntityValidatorThrowException_ThenNotModifyEntity_Helper<UserLeaveLimit, Guid>(
            FakeUserLeaveLimitProvider.GetLimits().ToList(),
            FakeUserLeaveLimitProvider.FakeLimitForOnDemandLeaveId);
        await WhenEntityValidatorThrowException_ThenNotModifyEntity_Helper<LeaveType, Guid>(
            FakeLeaveTypeProvider.GetLeaveTypes().ToList(),
            FakeLeaveTypeProvider.FakeSickLeaveId);
        await WhenEntityValidatorThrowException_ThenNotModifyEntity_Helper<Setting, string>(
            FakeSettingsProvider.GetSettings().ToList(),
            FakeSettingsProvider.AcceptedSettingId);
    }

    private async Task WhenEntityValidatorThrowException_ThenNotModifyEntity_Helper<TEntity, TId>(
        IReadOnlyCollection<TEntity> dataSource, TId key)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        var delta = new Delta<TEntity>();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var setMock = dataSource.AsQueryable().BuildMockDbSet();
        setMock.Setup(x => x.FindAsync(new object[] {key}, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSource.FirstOrDefault(x => x.Id.Equals(key)));
        dbContextMock.Setup(x => x.Set<TEntity>()).Returns(setMock.Object);
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        deltaValidatorMock.Setup(m => m.CreateDeltaWithoutProtectedProperties(delta)).Returns(delta);
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        entityValidatorMock.Setup(m => 
                m.ValidateAsync(It.IsAny<IValidationContext>(), new CancellationToken()))
            .ThrowsAsync(new Exception());
        var sut = GetSut<TEntity, TId>(dbContextMock.Object,deltaValidatorMock.Object, entityValidatorMock.Object);
        var act = async () =>
        {
            await sut.PatchAsync(key, new Delta<TEntity>());
        };
        await act.Should().ThrowAsync<Exception>();
        setMock.Verify(m => m.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()), Times.Once);
        deltaValidatorMock.Verify(m => m.CreateDeltaWithoutProtectedProperties(It.IsAny<Delta<TEntity>>()), Times.Once);
        entityValidatorMock.Verify(m => m.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()),Times.Once);
        dbContextMock.Verify(m => m.Set<TEntity>(), Times.Once);
        dbContextMock.Verify(m => m.Entry(It.IsAny<TEntity>()), Times.Never);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenExceptionWasThrownWhenSavingChangesAndEntityExists_ThenThrowDbUpdateConcurrencyException()
    {
        await WhenExceptionWasThrownWhenSavingChangesAndEntityExists_ThenThrowDbUpdateConcurrencyException_Helper<UserLeaveLimit, Guid>(
            FakeUserLeaveLimitProvider.GetLimits(),  FakeUserLeaveLimitProvider.GetLimitForOnDemandLeave());
        await WhenExceptionWasThrownWhenSavingChangesAndEntityExists_ThenThrowDbUpdateConcurrencyException_Helper<LeaveType, Guid>(
            FakeLeaveTypeProvider.GetLeaveTypes(), FakeLeaveTypeProvider.GetFakeSickLeave());
        await WhenExceptionWasThrownWhenSavingChangesAndEntityExists_ThenThrowDbUpdateConcurrencyException_Helper<Setting, string>(
            FakeSettingsProvider.GetSettings(), FakeSettingsProvider.GetPendingSetting()
        );
    }

    private async Task WhenExceptionWasThrownWhenSavingChangesAndEntityExists_ThenThrowDbUpdateConcurrencyException_Helper<TEntity, TId>(
        IEnumerable<TEntity> dataSource, TEntity entity)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        var delta = new Delta<TEntity>();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var setMock = dataSource.AsQueryable().BuildMockDbSet();
        setMock.Setup(x => x.FindAsync(new object[] {entity.Id}, default))
            .ReturnsAsync(entity);
        dbContextMock.Setup(x => x.Set<TEntity>()).Returns(setMock.Object);
        var entityEntryMock = new Mock<EntityEntry<TEntity>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
        dbContextMock.Setup(m => m.Entry(entity)).Returns(entityEntryMock.Object);
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        deltaValidatorMock.Setup(m => m.CreateDeltaWithoutProtectedProperties(delta)).Returns(delta);
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        var sut = GetSut<TEntity, TId>(dbContextMock.Object,deltaValidatorMock.Object, entityValidatorMock.Object);
        var act = async () =>
        {
            await sut.PatchAsync(entity.Id, delta);
        };
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
        setMock.Verify(m => m.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()), Times.Once);
        deltaValidatorMock.Verify(m => m.CreateDeltaWithoutProtectedProperties(It.IsAny<Delta<TEntity>>()), Times.Once);
        entityValidatorMock.Verify(m => m.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()),Times.Once);
        dbContextMock.Verify(m => m.Set<TEntity>(), Times.Exactly(2));
        dbContextMock.Verify(m => m.Entry(It.IsAny<TEntity>()), Times.Once);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenExceptionWasThrownWhenSavingChangesAndEntityNotExists_ThenThrowEntityNotFoundException()
    {
        var fakeUserLimits = new[]
        {
            FakeUserLeaveLimitProvider.GetLimitForHolidayLeave(),
            FakeUserLeaveLimitProvider.GetLimitForSickLeave()
        };
        await WhenExceptionWasThrownWhenSavingChangesAndEntityNotExists_ThenThrowEntityNotFoundException_Helper<UserLeaveLimit, Guid>(
            fakeUserLimits,  FakeUserLeaveLimitProvider.GetLimitForOnDemandLeave());
        var fakeLeaveTypes = new[]
        {
            FakeLeaveTypeProvider.GetFakeHolidayLeave(),
            FakeLeaveTypeProvider.GetFakeOnDemandLeave()
        };
        await WhenExceptionWasThrownWhenSavingChangesAndEntityNotExists_ThenThrowEntityNotFoundException_Helper<LeaveType, Guid>(
            fakeLeaveTypes, FakeLeaveTypeProvider.GetFakeSickLeave());
        var fakeSettings = new[]
        {
            FakeSettingsProvider.GetAcceptedSetting(),
            FakeSettingsProvider.GetCanceledSetting()
        };
        await WhenExceptionWasThrownWhenSavingChangesAndEntityNotExists_ThenThrowEntityNotFoundException_Helper<Setting, string>(
            fakeSettings, FakeSettingsProvider.GetPendingSetting()
        );
    }

    private async Task
        WhenExceptionWasThrownWhenSavingChangesAndEntityNotExists_ThenThrowEntityNotFoundException_Helper<TEntity, TId>(
            IEnumerable<TEntity> dataSource, TEntity entity)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        var delta = new Delta<TEntity>();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var setMock = dataSource.AsQueryable().BuildMockDbSet();
        setMock.Setup(x => x.FindAsync(new object[] {entity.Id}, default))
            .ReturnsAsync(entity);
        dbContextMock.Setup(x => x.Set<TEntity>()).Returns(setMock.Object);
        var entityEntryMock = new Mock<EntityEntry<TEntity>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
        dbContextMock.Setup(m => m.Entry(entity)).Returns(entityEntryMock.Object);
        dbContextMock.Setup(m => m.SaveChangesAsync(default))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        deltaValidatorMock.Setup(m => m.CreateDeltaWithoutProtectedProperties(delta)).Returns(delta);
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        var sut = GetSut<TEntity, TId>(dbContextMock.Object,deltaValidatorMock.Object, entityValidatorMock.Object);
        var act = async () =>
        {
            await sut.PatchAsync(entity.Id, delta);
        };
        await act.Should().ThrowAsync<EntityNotFoundException>();
        setMock.Verify(m => m.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()), Times.Once);
        deltaValidatorMock.Verify(m => m.CreateDeltaWithoutProtectedProperties(It.IsAny<Delta<TEntity>>()), Times.Once);
        entityValidatorMock.Verify(m => m.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()),Times.Once);
        dbContextMock.Verify(m => m.Set<TEntity>(), Times.Exactly(2));
        dbContextMock.Verify(m => m.Entry(It.IsAny<TEntity>()), Times.Once);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenNoProblems_ThenReturnModifiedEntity()
    {
        await WhenNoProblems_ThenReturnModifiedEntity_Helper<UserLeaveLimit, Guid>(
            FakeUserLeaveLimitProvider.GetLimits(),  FakeUserLeaveLimitProvider.GetLimitForOnDemandLeave());
        await WhenNoProblems_ThenReturnModifiedEntity_Helper<LeaveType, Guid>(
            FakeLeaveTypeProvider.GetLeaveTypes(), FakeLeaveTypeProvider.GetFakeSickLeave());
        await WhenNoProblems_ThenReturnModifiedEntity_Helper<Setting, string>(
            FakeSettingsProvider.GetSettings(), FakeSettingsProvider.GetPendingSetting());
    }

    private async Task WhenNoProblems_ThenReturnModifiedEntity_Helper<TEntity, TId>(
        IEnumerable<TEntity> dataSource, TEntity entity)
        where TId : IComparable<TId>
        where TEntity : class, IHaveId<TId>
    {
        var delta = new Delta<TEntity>();
        var dbContextMock = new Mock<LeaveSystemDbContext>(new DbContextOptions<LeaveSystemDbContext>());
        var setMock = dataSource.AsQueryable().BuildMockDbSet();
        setMock.Setup(x => x.FindAsync(new object[] {entity.Id}, default))
            .ReturnsAsync(entity);
        dbContextMock.Setup(x => x.Set<TEntity>()).Returns(setMock.Object);
        var entityEntryMock = new Mock<EntityEntry<TEntity>>(FormatterServices.GetUninitializedObject(typeof(InternalEntityEntry)));
        dbContextMock.Setup(m => m.Entry(entity)).Returns(entityEntryMock.Object);
        var deltaValidatorMock = new Mock<DeltaValidator<TEntity>>();
        deltaValidatorMock.Setup(m => m.CreateDeltaWithoutProtectedProperties(delta)).Returns(delta);
        var entityValidatorMock = new Mock<IValidator<TEntity>>();
        var sut = GetSut<TEntity, TId>(dbContextMock.Object,deltaValidatorMock.Object, entityValidatorMock.Object);
        var result = await sut.PatchAsync(entity.Id, delta);
        result.Should().BeEquivalentTo(entity);
        setMock.Verify(m => m.FindAsync(It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()), Times.Once);
        deltaValidatorMock.Verify(m => m.CreateDeltaWithoutProtectedProperties(It.IsAny<Delta<TEntity>>()), Times.Once);
        entityValidatorMock.Verify(m => m.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()),Times.Once);
        dbContextMock.Verify(m => m.Set<TEntity>(), Times.Once);
        dbContextMock.Verify(m => m.Entry(It.IsAny<TEntity>()), Times.Once);
        dbContextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}