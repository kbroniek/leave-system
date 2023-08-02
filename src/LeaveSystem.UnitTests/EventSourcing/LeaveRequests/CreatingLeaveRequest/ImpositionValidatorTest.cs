using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.TestHelpers;
using Marten;
using Marten.Events;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using Xunit;
using LeaveSystem.UnitTests.Stubs;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class ImpositionValidatorTest
{
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;
    private readonly Mock<WorkingHoursService> workingHoursServiceMock = new();
    private readonly Mock<IDocumentSession> documentSessionMock = new();
    private readonly Mock<IEventStore> eventStoreMock = new();

    private readonly LeaveRequestCreated fakeLeaveRequestCreatedEvent =
        FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();

    private readonly FederatedUser fakeUser = FakeUserProvider.GetUserWithNameFakeoslav();

    public ImpositionValidatorTest()
    {
        documentSessionMock.SetupGet(v => v.Events)
            .Returns(eventStoreMock.Object);
    }

    [Fact]
    public async Task WhenThereIsOtherValidLeveRequestWithSameId_ThenThrowValidationException()
    {
        //Given
        var events = FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedEventsWithDifferentIds();
        var fakeEvent =
            FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedCalculatedFromCurrentDate(WorkingHours * 2,
                FakeLeaveTypeProvider.FakeSickLeaveId);
        events.Add(fakeEvent);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock.SetupLimitValidatorFunctions(new MartenQueryableStub<LeaveRequestCreated>(events), entity);
        await using var dbContext = await DbContextFactory.CreateAndFillDbAsync();
        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().ThrowAsync<ValidationException>();
        documentSessionMock.VerifyLeaveRequestValidatorFunctions(entity.Id, Times.Once(), Times.AtLeastOnce());
    }

    private CreateLeaveRequestValidator GetSut(LeaveSystemDbContext dbContext) =>
        new(dbContext, workingHoursServiceMock.Object, documentSessionMock.Object);

    [Fact]
    public async Task
        WhenThereIsNotOtherLeveRequestWithSameId_ThenNotThrowValidationException()
    {
        //Given
        var events = FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedEventsWithDifferentIds();
        var fakeEvent =
            FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedCalculatedFromCurrentDate(WorkingHours * 2,
                FakeLeaveTypeProvider.FakeSickLeaveId);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock.SetupLimitValidatorFunctions(new MartenQueryableStub<LeaveRequestCreated>(events), entity);
        await using var dbContext = await DbContextFactory.CreateAndFillDbAsync();
        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().NotThrowAsync<ValidationException>();
        documentSessionMock.VerifyLeaveRequestValidatorFunctions(entity.Id, Times.Once(), Times.Never());
    }

    [Fact]
    public async Task
        WhenOtherLeaveRequestWithSameIdIsNotValid_ThenNotThrowValidationException()
    {
        //Given
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeLeaveRequestCreatedEvent);
        entity.Cancel("cancel fake remarks", fakeUser);
        var events = FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedEventsWithDifferentIds();
        events.Add(fakeLeaveRequestCreatedEvent);
        eventStoreMock.SetupLimitValidatorFunctions(new MartenQueryableStub<LeaveRequestCreated>(events), entity);
        await using var dbContext = await DbContextFactory.CreateAndFillDbAsync();
        var sut = GetSut(dbContext);
        //When
        var act = async () => { await sut.ImpositionValidator(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().NotThrowAsync<ValidationException>();
        documentSessionMock.VerifyLeaveRequestValidatorFunctions(entity.Id, Times.Once(), Times.AtLeastOnce());
    }
}