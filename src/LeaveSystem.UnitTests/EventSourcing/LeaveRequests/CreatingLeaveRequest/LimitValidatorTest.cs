using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Extensions;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using LeaveSystem.UnitTests.TestHelpers;
using Marten;
using Marten.Events;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class LimitValidatorTest
{
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;
    private readonly Mock<WorkingHoursService> workingHoursServiceMock = new ();
    private readonly Mock<IDocumentSession> documentSessionMock = new ();
    private readonly Mock<IEventStore> eventStoreMock = new ();
    private readonly LeaveRequestCreated fakeLeaveRequestCreatedEvent =
        FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();
    private readonly FederatedUser fakeUser = FakeUserProvider.GetUserWithNameFakeoslav();

    public LimitValidatorTest()
    {
        documentSessionMock.SetupGet(v => v.Events)
            .Returns(eventStoreMock.Object);
    }

    public static IEnumerable<object[]> GetFakeLeaveRequestCreatedTestData()
    {
        yield return new object[]
        {
            FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedWithSickLeave()
        };
        yield return new object[]
        {
            FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreated()
        };
        yield return new object[]
        {
            FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedWithOnDemandLeave()
        };
    }

    [Theory]
    [MemberData(nameof(GetFakeLeaveRequestCreatedTestData))]
    public async Task
        WhenCreatedLeaveTypeIsOutOfBaseLeaveTypeOrNestedLeaveType_ThenThrowValidationException(LeaveRequestCreated @event)
    {
        //Given
        var events = FakeLeaveRequestCreatedProvider.GetMartenQueryableStub();
        var leaveRequestEntity = LeaveRequest.CreatePendingLeaveRequest(fakeLeaveRequestCreatedEvent);
        eventStoreMock.SetupLimitValidatorFunctions(new MartenQueryableStub<LeaveRequestCreated>(events), leaveRequestEntity);
        await using var dbContext = await CreateAndFillDbAsync();
        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.LimitValidator(@event); };
        //Then
        await act.Should().ThrowAsync<ValidationException>().WithMessage("You don't have enough free days for this type of leave");
        VerifyDocumentSessionMockCalled(leaveRequestEntity.Id, Times.Once(), Times.Exactly(2));
    }

    private void VerifyDocumentSessionMockCalled(Guid leaveRequestId, Times queryRawEventDataOnlyTimes, Times aggregateStreamAsyncTimes)
    {
        documentSessionMock.VerifyLeaveRequestValidatorFunctions(leaveRequestId, queryRawEventDataOnlyTimes, aggregateStreamAsyncTimes);
    }

    private async Task<LeaveSystemDbContext> CreateAndFillDbAsync()
    {
        var dbContext = await DbContextFactory.CreateDbContextAsync();
        await AddLeaveTypesToDbAsync(dbContext);
        await dbContext.SaveChangesAsync();
        await AddUserLeaveLimitsToDbAsync(dbContext);
        await dbContext.SaveChangesAsync();
        return dbContext;
    }

    private async Task AddLeaveTypesToDbAsync(LeaveSystemDbContext dbContext)
    {
        await dbContext.LeaveTypes.AddRangeAsync(
            FakeLeaveTypeProvider.GetLeaveTypes()
        );
    }

    private async Task AddUserLeaveLimitsToDbAsync(LeaveSystemDbContext dbContext)
    {
        await dbContext.UserLeaveLimits.AddRangeAsync(FakeUserLeaveLimitProvider.GetLimits());
    }

    private CreateLeaveRequestValidator GetSut(LeaveSystemDbContext dbContext) =>
        new(dbContext, workingHoursServiceMock.Object, documentSessionMock.Object);

    [Fact]
    public async Task
        WhenCreatedLeaveTypeOrIsWithinTheLimit_ThenNotThrowValidationException()
    {
        //Given
        var events = FakeLeaveRequestCreatedProvider.GetMartenQueryableStub();
        var fakeEvent = FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedCalculatedFromCurrentDate(WorkingHours * 2, FakeLeaveTypeProvider.FakeHolidayLeaveGuid);
        var leaveRequestEntity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock.SetupLimitValidatorFunctions(new MartenQueryableStub<LeaveRequestCreated>(events), leaveRequestEntity);
        await using var dbContext = await DbContextFactory.CreateAndFillDbAsync();
        var sut = GetSut(dbContext);
        var @event = LeaveRequestCreated.Create(
            fakeLeaveRequestCreatedEvent.LeaveRequestId,
            fakeLeaveRequestCreatedEvent.DateFrom,
            fakeLeaveRequestCreatedEvent.DateTo,
            WorkingHours,
            FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
            fakeLeaveRequestCreatedEvent.Remarks,
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        //When
        var act = async () => { await sut.LimitValidator(@event); };
        //Then
        await act.Should().NotThrowAsync<ValidationException>();
        VerifyDocumentSessionMockCalled(leaveRequestEntity.Id, Times.Exactly(2), Times.Exactly(2));
    }

    [Fact]
    public async Task
        WhenNoLimitForLeaveRequestCreated_ThenThrowValidationException()
    {
        //Given
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.LimitValidator(fakeLeaveRequestCreatedEvent); };
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Cannot find limits for the leave type id*");
        VerifyDocumentSessionMockCalled(It.IsAny<Guid>(), Times.Never(), Times.Never());
    }

    [Fact]
    public async Task
        WhenMoreThanOneLimitForLeaveRequestCreated_ThenThrowValidationException()
    {
        //Given
        await using var dbContext = await CreateAndFillDbAsync();
        var now = DateTimeOffset.Now;
        var secondLeaveLimitForSameUser = new UserLeaveLimit
        {
            Id = Guid.Parse("8e9709d4-c237-4043-bde3-bb58bca35c2e"),
            LeaveTypeId = FakeLeaveTypeProvider.FakeSickLeaveId,
            Limit = FakeLeaveTypeProvider.GetFakeSickLeave().Properties?.DefaultLimit,
            AssignedToUserId = FakeUserProvider.GetUserWithNameFakeoslav().Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
        await dbContext.UserLeaveLimits.AddAsync(secondLeaveLimitForSameUser);
        await dbContext.SaveChangesAsync();
        var sut = GetSut(dbContext);
        var @event = LeaveRequestCreated.Create(
            fakeLeaveRequestCreatedEvent.LeaveRequestId,
            fakeLeaveRequestCreatedEvent.DateFrom,
            fakeLeaveRequestCreatedEvent.DateTo,
            WorkingHours * 3,
            FakeLeaveTypeProvider.FakeSickLeaveId,
            fakeLeaveRequestCreatedEvent.Remarks,
            fakeUser
        );
        //When
        var act = async () => { await sut.LimitValidator(@event); };
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Two or more limits found which are the same for the leave type id*");
        VerifyDocumentSessionMockCalled(It.IsAny<Guid>(), Times.Never(), Times.Never());
    }
}