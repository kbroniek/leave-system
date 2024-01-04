using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.CreatingWorkingHours;

public class WorkingHoursFactoryTest
{

    private static WorkingHoursFactory GetSut() => new();

    [Fact]
    public void WhenPassedWithoutExceptions_ThenReturnWorkingHours()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        var sut = GetSut();
        //When
        var result = sut.Create(command);
        //Then
        result.Should().BeEquivalentTo(new
        {
            Id = command.WorkingHoursId,
            command.UserId,
            command.DateFrom,
            command.DateTo,
            command.Duration
        });
        result.DequeueUncommittedEvents().Should().BeEquivalentTo(new[]
        {
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom, command.DateTo, command.Duration, command.CreatedBy)
        });
    }
}