using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Exceptions;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;
using LeaveSystem.Shared.Date;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LeaveRequestFactoryTest
{
    private static readonly TimeSpan WorkingHours = TimeSpan.FromHours(8);

    private static LeaveRequestFactory GetSut(
        LeaveSystemDbContext dbContext, IQuerySession querySession, CreateLeaveRequestValidator validator)
    {
        return new LeaveRequestFactory(validator, dbContext, querySession);
    }

    private static readonly DateTimeOffset Now = DateTimeOffset.Now;

    [Fact]
    public async Task WhenLeaveTypeNotFound_ThenThrowNotFoundException()
    {
        //Given
        var fakeEvent = CreateLeaveRequest.Create(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 7, 27, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 8, 5, 0, 0, 0, TimeSpan.Zero),
            WorkingHours * 3,
            FakeLeaveTypeProvider.FakeSickLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddAsync(FakeLeaveTypeProvider.GetFakeOnDemandLeave());
        var querySessionMock = new Mock<IQuerySession>();
        var martenQueryableStub = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            FakeWorkingHoursProvider.GetAll(Now)
            );
        querySessionMock.Setup(m => m.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>())
            .Returns(martenQueryableStub);
        var validatorMock = GetValidatorMock();
        var sut = GetSut(dbContext, querySessionMock.Object, validatorMock.Object);
        //When
        var act = async () => { await sut.Create(fakeEvent, It.IsAny<CancellationToken>()); };
        //Then
        await act.Should().ThrowAsync<Ardalis.GuardClauses.NotFoundException>();
        querySessionMock.Verify(x => x.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Times.Never);
        validatorMock.Verify(x => x.Validate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Never
        );
    }

    private Mock<CreateLeaveRequestValidator> GetValidatorMock() =>
        new(null, null, null);

    [Fact]
    public async Task WhenDurationGreaterThanYear_ThenThrowArgumentOutOfRangeException()
    {
        //Given
        var fakeEvent = CreateLeaveRequest.Create(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 7, 27, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 8, 5, 0, 0, 0, TimeSpan.Zero),
            WorkingHours * 3,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddAsync(FakeLeaveTypeProvider.GetFakeOnDemandLeave());
        var querySessionMock = new Mock<IQuerySession>();
        var martenQueryableStub = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            FakeWorkingHoursProvider.GetAll(Now)
        );
        querySessionMock.Setup(m => m.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>())
            .Returns(martenQueryableStub);
        var validatorMock = GetValidatorMock();
        var sut = GetSut(dbContext, querySessionMock.Object, validatorMock.Object);
        //When
        var act = async () => { await sut.Create(fakeEvent, It.IsAny<CancellationToken>()); };
        //Then
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        querySessionMock.Verify(x => x.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Times.Once);
        validatorMock.Verify(x => x.Validate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Never
        );
    }

    [Fact]
    public async Task WhenCommandValid_ThenCreateLeaveRequest()
    {
        var fakeEvent = CreateLeaveRequest.Create(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 7, 27, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 8, 5, 0, 0, 0, TimeSpan.Zero),
            WorkingHours * 3,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddAsync(FakeLeaveTypeProvider.GetFakeOnDemandLeave());
        var querySessionMock = new Mock<IQuerySession>();
        var martenQueryableStub = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            FakeWorkingHoursProvider.GetAll(Now)
        );
        querySessionMock.Setup(m => m.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>())
            .Returns(martenQueryableStub);
        var validatorMock = GetValidatorMock();
        var sut = GetSut(dbContext, querySessionMock.Object, validatorMock.Object);
        //When
        var leaveRequest = await sut.Create(fakeEvent, It.IsAny<CancellationToken>());
        //Then
        querySessionMock.Verify(x => x.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>());
        validatorMock.Verify(x => x.Validate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Once
        );
        leaveRequest.Should().BeEquivalentTo(new
        {
            Id = fakeEvent.LeaveRequestId,
            DateFrom = fakeEvent.DateFrom,
            DateTo = fakeEvent.DateTo,
            Duration = fakeEvent.Duration,
            LeaveTypeId = fakeEvent.LeaveTypeId,
            Remarks = new List<LeaveRequest.RemarksModel>() {new (fakeEvent.Remarks!, fakeEvent.CreatedBy)},
            Status = LeaveRequestStatus.Pending,
            CreatedBy = fakeEvent.CreatedBy,
            LastModifiedBy = fakeEvent.CreatedBy,
            
        },o => o.ExcludingMissingMembers());
    }

    [Fact]
    public async Task WhenNoWorkingHoursForCreator_ThenThrowInvalidOperationException()
    {
                var fakeEvent = CreateLeaveRequest.Create(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 7, 27, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 8, 5, 0, 0, 0, TimeSpan.Zero),
            WorkingHours * 3,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddAsync(FakeLeaveTypeProvider.GetFakeOnDemandLeave());
        var querySessionMock = new Mock<IQuerySession>();
        var martenQueryableStub = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            new [] {FakeWorkingHoursProvider.GetCurrentForBen(DateTimeOffset.Now)}
        );
        querySessionMock.Setup(m => m.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>())
            .Returns(martenQueryableStub);
        var validatorMock = GetValidatorMock();
        var sut = GetSut(dbContext, querySessionMock.Object, validatorMock.Object);
        //When
        var act = async () => { await sut.Create(fakeEvent, It.IsAny<CancellationToken>()); };
        //Then
        await act.Should().ThrowAsync<InvalidOperationException>();
        querySessionMock.Verify(x => x.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>());
        validatorMock.Verify(x => x.Validate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Never
        );
    }
}