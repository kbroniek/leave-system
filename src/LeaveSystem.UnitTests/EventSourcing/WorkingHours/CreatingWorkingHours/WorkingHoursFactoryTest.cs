using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using Marten;
using NSubstitute;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.CreatingWorkingHours;

public class WorkingHoursFactoryTest
{
    private CreateWorkingHoursValidator validator;

    private WorkingHoursFactory GetSut() => new(validator);

    [Fact]
    public void WhenValidateWorkingHoursUniqueThrowsException_ThenThrowIt()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        validator = Substitute.For<CreateWorkingHoursValidator>(Substitute.For<IDocumentSession>());
        validator.When(m => m.ValidateWorkingHoursUnique(Arg.Any<WorkingHoursCreated>()))
            .Throw<ValidationException>();
        var sut = GetSut();
        //When
        var act = () => { sut.Create(command); };
        //Then
        act.Should().Throw<ValidationException>();
        validator.Received().ValidateWorkingHoursUnique(Arg.Any<WorkingHoursCreated>());
    }

    [Fact]
    public void WhenPassedWithoutExceptions_ThenReturnWorkingHours()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        validator = Substitute.For<CreateWorkingHoursValidator>(Substitute.For<IDocumentSession>());
        var sut = GetSut();
        //When
        var result = sut.Create(command);
        //Then
        result.Should().BeEquivalentTo(new
            {
                Id = command.WorkingHoursId,
                UserId = command.UserId,
                DateFrom = command.DateFrom,
                DateTo = command.DateTo,
                Duration = command.Duration,
                Status = WorkingHoursStatus.Current
            }
        );
        result.DequeueUncommittedEvents().Should().BeEquivalentTo(new[]
        {
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom, command.DateTo, command.Duration)
        });
        validator.Received().ValidateWorkingHoursUnique(Arg.Any<WorkingHoursCreated>());
    }
}