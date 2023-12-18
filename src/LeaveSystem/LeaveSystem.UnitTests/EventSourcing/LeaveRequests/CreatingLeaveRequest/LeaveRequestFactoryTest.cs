using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LeaveRequestFactoryTest
{
    private static readonly TimeSpan WorkingHours = TimeSpan.FromHours(8);


    private static readonly DateTimeOffset FakeNow = FakeDateServiceProvider.GetDateService().GetWithoutTime();

    [Fact]
    public async Task WhenLeaveTypeNotFound_ThenThrowNotFoundException()
    {
        //Given
        var fakeEvent = CreateLeaveRequest.Create(
            Guid.Parse("1c2782ae-1068-4a6d-bd64-7850a7aab0e1"),
            DateTimeOffset.Parse("2023-12-16"),
            DateTimeOffset.Parse("2023-12-16"),
            WorkingHours,
            Guid.Parse("1c2782ae-1068-4a6d-bd64-7850a7aab0e2"),
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var querySessionMock = new Mock<IQuerySession>();
        var validatorMock = GetValidatorMock();
        var sut = GetSut(dbContext, querySessionMock.Object, validatorMock.Object);
        //When
        var act = async () => { await sut.Create(fakeEvent, It.IsAny<CancellationToken>()); };
        //Then
        var result = await act.Should().ThrowAsync<Ardalis.GuardClauses.NotFoundException>();
        result.WithMessage("Queried object leaveTypeId was not found, Key: 1c2782ae-1068-4a6d-bd64-7850a7aab0e2");
        querySessionMock.Verify(x => x.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Times.Never);
        validatorMock.Verify(x => x.Validate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Never
        );
    }

    [Fact]
    public async Task WhenNoWorkingHoursForCreator_ThenThrowInvalidOperationException()
    {
        var fakeEvent = CreateLeaveRequest.Create(
            Guid.Parse("1c2782ae-1068-4a6d-bd64-7850a7aab0e3"),
            DateTimeOffset.Parse("2023-12-16"),
            DateTimeOffset.Parse("2023-12-16"),
            WorkingHours,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        await using var dbContext = await GetDbContext();
        var querySessionMock = GetQuerySessionMock(new[] { FakeWorkingHoursProvider.GetCurrentForBen(DateTimeOffset.Parse("2023-12-15")) });
        var validatorMock = GetValidatorMock();
        var sut = GetSut(dbContext, querySessionMock.Object, validatorMock.Object);
        //When
        var act = async () => { await sut.Create(fakeEvent, It.IsAny<CancellationToken>()); };
        //Then
        var result = await act.Should().ThrowAsync<InvalidOperationException>();
        result.WithMessage("User with ID 1 does not have working Hours");
        querySessionMock.Verify(x => x.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(), Times.Once);
        validatorMock.Verify(x => x.Validate(
                It.IsAny<LeaveRequestCreated>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<bool?>()
            ), Times.Never
        );
    }
    [Fact]
    public async Task WhenDurationGreaterThanYear_ThenThrowArgumentOutOfRangeException()
    {
        //Given
        var fakeEvent = CreateLeaveRequest.Create(
            Guid.Parse("1c2782ae-1068-4a6d-bd64-7850a7aab0e5"),
            DateTimeOffset.Parse("2023-07-27"),
            DateTimeOffset.Parse("2025-08-05"),
            WorkingHours * 3,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        await using var dbContext = await GetDbContext();
        var querySessionMock = GetQuerySessionMock();
        var validatorMock = GetValidatorMock();
        var sut = GetSut(dbContext, querySessionMock.Object, validatorMock.Object);
        //When
        var act = async () => { await sut.Create(fakeEvent, It.IsAny<CancellationToken>()); };
        //Then
        var result = await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        result.WithMessage("Specified argument was out of the range of valid values. (Parameter 'Max range reached calculating duration between dates from: 2023-07-27T00:00:00.0000000+00:00 to: 2025-08-05T00:00:00.0000000+02:00. The max duration is 366 days.')");
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
            Guid.Parse("1c2782ae-1068-4a6d-bd64-7850a7aab0e6"),
            DateTimeOffset.Parse("2023-07-27"),
            DateTimeOffset.Parse("2023-08-05"),
            WorkingHours * 3,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            "fake remarks",
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddAsync(FakeLeaveTypeProvider.GetFakeOnDemandLeave());
        var querySessionMock = GetQuerySessionMock();
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
            DateFrom = fakeEvent.DateFrom.GetDayWithoutTime(),
            DateTo = fakeEvent.DateTo.GetDayWithoutTime(),
            Duration = fakeEvent.Duration,
            LeaveTypeId = fakeEvent.LeaveTypeId,
            Remarks = new List<LeaveRequest.RemarksModel>() { new(fakeEvent.Remarks!, fakeEvent.CreatedBy) },
            Status = LeaveRequestStatus.Pending,
            CreatedBy = fakeEvent.CreatedBy,
            LastModifiedBy = fakeEvent.CreatedBy,

        }, o => o.ExcludingMissingMembers());
    }

    private static LeaveRequestFactory GetSut(
        LeaveSystemDbContext dbContext, IQuerySession querySession, CreateLeaveRequestValidator validator)
    {
        return new LeaveRequestFactory(validator, dbContext, querySession, FakeDateServiceProvider.GetDateService());
    }
    private static Mock<CreateLeaveRequestValidator> GetValidatorMock() => new(null, null, null);

    private static Mock<IQuerySession> GetQuerySessionMock(IEnumerable<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>? workingHours = null)
    {
        var querySessionMock = new Mock<IQuerySession>();
        var martenQueryableStub = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            workingHours ?? FakeWorkingHoursProvider.GetAll(FakeNow)
        );
        querySessionMock.Setup(m => m.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>())
            .Returns(martenQueryableStub);
        return querySessionMock;
    }

    private static async Task<LeaveSystemDbContext> GetDbContext()
    {
        var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.LeaveTypes.AddAsync(FakeLeaveTypeProvider.GetFakeOnDemandLeave());
        return dbContext;
    }
}