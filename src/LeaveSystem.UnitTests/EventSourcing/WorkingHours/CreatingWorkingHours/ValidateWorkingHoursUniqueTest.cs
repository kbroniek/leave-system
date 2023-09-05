using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using Marten.Events;
using NSubstitute;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.CreatingWorkingHours;

public class ValidateWorkingHoursUniqueTest
{
    private IDocumentSession documentSession;

    private CreateWorkingHoursValidator GetSut() => new(documentSession);

    [Fact]
    public void WhenWorkingHoursWithThisUserIdWasCreated_ThenThrowValidationException()
    {
        //Given
        var martenQueryableStub = new MartenQueryableStub<WorkingHoursCreated>(
            FakeWorkingHoursCreatedProvider.GetAll()
        );
        var eventStoreMock = Substitute.For<IEventStore>();
        eventStoreMock.QueryRawEventDataOnly<WorkingHoursCreated>().Returns(martenQueryableStub);
        documentSession = Substitute.For<IDocumentSession>();
        documentSession.Events.Returns(eventStoreMock);
        var sut = GetSut();
        var fakeEvent = FakeWorkingHoursCreatedProvider.GetForBen();
        //When
        var act = () =>
        {
            sut.ValidateWorkingHoursUnique(fakeEvent);
        };
        //Then
        act.Should().Throw<ValidationException>();
        documentSession.Received(1).Events.QueryRawEventDataOnly<WorkingHoursCreated>();
    }
    
    [Fact]
    public void WhenWorkingHoursWithThoseUserIdWasNotCreated_ThenNotThrowThrowValidationException()
    {
        //Given
        var martenQueryableStub = new MartenQueryableStub<WorkingHoursCreated>(
            FakeWorkingHoursCreatedProvider.GetAll(false)
        );
        var eventStoreMock = Substitute.For<IEventStore>();
        eventStoreMock.QueryRawEventDataOnly<WorkingHoursCreated>().Returns(martenQueryableStub);
        documentSession = Substitute.For<IDocumentSession>();
        documentSession.Events.Returns(eventStoreMock);
        var sut = GetSut();
        var fakeEvent = FakeWorkingHoursCreatedProvider.GetForBen();
        //When
        var act = () =>
        {
            sut.ValidateWorkingHoursUnique(fakeEvent);
        };
        //Then
        act.Should().NotThrow<ValidationException>();
        documentSession.Received(1).Events.QueryRawEventDataOnly<WorkingHoursCreated>();
    }
}