using System;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequestCreatedTest
{
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;

    [Fact]
    public void WhenEmailIsInvalid_ThenThrowArgumentException()
    {
        //Given
        var fakeUser = FederatedUser.Create("1", "@wrong.email@fakecom.", "John");
        //When
        var act = () =>
        {
            LeaveRequestCreated.Create(
                Guid.NewGuid(),
                new DateTimeOffset(2023, 7, 27, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2023, 7, 30, 0, 0, 0, TimeSpan.Zero),
                WorkingHours * 3,
                Guid.NewGuid(),
                "fake remarks",
                fakeUser
            );
        };
        //Then
        act.Should().Throw<ArgumentException>();
    }
    //Todo: Finish tests for this Method
    [Fact]
    public void WhenDateIsOutOfYear_ThenThrowArgumentOutOfRangeException()
    {
        
    }
}