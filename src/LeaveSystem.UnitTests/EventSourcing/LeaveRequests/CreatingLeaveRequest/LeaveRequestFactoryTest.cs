using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.TestHelpers;
using Marten;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LeaveRequestFactoryTest
{
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;

    private static LeaveRequestFactory GetSut(
        LeaveSystemDbContext dbContext, WorkingHoursService workingHoursService, CreateLeaveRequestValidator validator)
    {
        return new LeaveRequestFactory(validator, workingHoursService, dbContext);
    }

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
        var workingHoursMock = new Mock<WorkingHoursService>();
        var validatorMock = GetValidatorMock(workingHoursMock.Object, dbContext);
        var sut = GetSut(dbContext, workingHoursMock.Object, validatorMock.Object);
        //When
        var act = async () => { await sut.Create(fakeEvent, It.IsAny<CancellationToken>()); };
        //Then
        await act.Should().ThrowAsync<Ardalis.GuardClauses.NotFoundException>();
        workingHoursMock.Verify(x => x.GetUserSingleWorkingHoursDuration(
                It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()
            ), Times.Never
        );
        validatorMock.Verify(x => x.BasicValidate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Never
        );
        validatorMock.Verify(x => x.ImpositionValidator(It.IsAny<LeaveRequestCreated>()), Times.Never);
        validatorMock.Verify(x => x.LimitValidator(It.IsAny<LeaveRequestCreated>()), Times.Never);
    }

    private Mock<CreateLeaveRequestValidator> GetValidatorMock(WorkingHoursService workingHoursService, LeaveSystemDbContext dbContext) =>
        new(dbContext, workingHoursService, new Mock<IDocumentSession>().Object);

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
        var workingHoursServiceMock = new Mock<WorkingHoursService>();
        workingHoursServiceMock.SetupGetUserSingleWorkingHoursDuration(
            fakeEvent.CreatedBy.Id, fakeEvent.DateFrom, fakeEvent.DateTo, fakeEvent.Duration!.Value);
        var validatorMock = GetValidatorMock(workingHoursServiceMock.Object, dbContext);
        var sut = GetSut(dbContext, workingHoursServiceMock.Object, validatorMock.Object);
        //When
        var act = async () => { await sut.Create(fakeEvent, It.IsAny<CancellationToken>()); };
        //Then
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        workingHoursServiceMock.Verify(x => x.GetUserSingleWorkingHoursDuration(
                It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()
            ), Times.Once
        );
        validatorMock.Verify(x => x.BasicValidate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Never
        );
        validatorMock.Verify(x => x.LimitValidator(It.IsAny<LeaveRequestCreated>()), Times.Never);
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
        var workingHoursServiceMock = new Mock<WorkingHoursService>();
        workingHoursServiceMock.SetupGetUserSingleWorkingHoursDuration(
            fakeEvent.CreatedBy.Id, fakeEvent.DateFrom, fakeEvent.DateTo, fakeEvent.Duration!.Value);
        var validatorMock = GetValidatorMock(workingHoursServiceMock.Object, dbContext);
        var sut = GetSut(dbContext, workingHoursServiceMock.Object, validatorMock.Object);
        //When
        var leaveRequest = await sut.Create(fakeEvent, It.IsAny<CancellationToken>());
        //Then
        workingHoursServiceMock.Verify(x => x.GetUserSingleWorkingHoursDuration(
                It.IsAny<string>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()
            ), Times.Once
        );
        validatorMock.Verify(x => x.BasicValidate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Once
        );
        validatorMock.Verify(x => x.LimitValidator(It.IsAny<LeaveRequestCreated>()), Times.Once);
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
}