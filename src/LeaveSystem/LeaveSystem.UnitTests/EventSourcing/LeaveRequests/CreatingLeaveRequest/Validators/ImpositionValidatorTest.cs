using System.ComponentModel.DataAnnotations;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Extensions;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using Marten.Events;
using Moq;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class ImpositionValidatorTest
{
    private static readonly TimeSpan WorkingHours = TimeSpan.FromHours(8);
    private readonly Mock<IDocumentSession> documentSessionMock = new();
    private readonly Mock<IEventStore> eventStoreMock = new();

    private readonly LeaveRequestCreated fakeLeaveRequestCreatedEvent =
        FakeLeaveRequestCreatedProvider.GetLeaveRequestWithHolidayLeaveCreatedCalculatedFromCurrentDate();

    private readonly FederatedUser fakeUser = FakeUserProvider.GetUserWithNameFakeoslav();

    private ImpositionValidator GetSut() => new(documentSessionMock.Object);

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
            FakeLeaveRequestCreatedProvider.GetHolodayLeaveRequest(WorkingHours * 2,
                FakeLeaveTypeProvider.FakeSickLeaveId);
        events.Add(fakeEvent);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock.SetupLimitValidatorFunctions(new MartenQueryableStub<LeaveRequestCreated>(events), entity);
        await using var dbContext = await DbContextFactory.CreateAndFillDbAsync();
        var sut = GetSut();
        //When
        var act = async () => { await sut.Validate(fakeLeaveRequestCreatedEvent); };
        //Then
        var result = await act.Should().ThrowAsync<ValidationException>();
        result.WithMessage("Cannot create a new leave request in this time. The other leave is overlapping with this date.");
        documentSessionMock.VerifyLeaveRequestValidatorFunctions(entity.Id, Times.Once(), Times.AtLeastOnce());
    }

    [Fact]
    public async Task
        WhenThereIsNotOtherLeveRequestWithSameId_ThenNotThrowValidationException()
    {
        //Given
        var events = FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedEventsWithDifferentIds();
        var fakeEvent =
            FakeLeaveRequestCreatedProvider.GetHolodayLeaveRequest(WorkingHours * 2,
                FakeLeaveTypeProvider.FakeSickLeaveId);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock.SetupLimitValidatorFunctions(new MartenQueryableStub<LeaveRequestCreated>(events), entity);
        await using var dbContext = await DbContextFactory.CreateAndFillDbAsync();
        var sut = GetSut();
        //When
        var act = async () => { await sut.Validate(fakeLeaveRequestCreatedEvent); };
        //Then
        var result = await act.Should().NotThrowAsync<ValidationException>();
        documentSessionMock.VerifyLeaveRequestValidatorFunctions(entity.Id, Times.Once(), Times.Never());
    }

    [Fact]
    public async Task
        WhenOtherLeaveRequestWithSameIdIsNotValid_ThenNotThrowValidationException()
    {
        //Given
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeLeaveRequestCreatedEvent);
        entity.Cancel("cancel fake remarks", fakeUser, DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        var events = FakeLeaveRequestCreatedProvider.GetLeaveRequestCreatedEventsWithDifferentIds();
        events.Add(fakeLeaveRequestCreatedEvent);
        eventStoreMock.SetupLimitValidatorFunctions(new MartenQueryableStub<LeaveRequestCreated>(events), entity);
        var sut = GetSut();
        //When
        var act = async () => { await sut.Validate(fakeLeaveRequestCreatedEvent); };
        //Then
        await act.Should().NotThrowAsync<ValidationException>();
        documentSessionMock.VerifyLeaveRequestValidatorFunctions(entity.Id, Times.Once(), Times.AtLeastOnce());
    }
}
