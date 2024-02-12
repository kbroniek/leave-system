using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class CreateLeaveRequestCreatedTest
{
    [Fact]
    public void WhenEmailIsInvalid_ThenThrowArgumentException()
    {
        //Given
        var fakeUser = FederatedUser.Create("1", "@wrong.email@fakecom.", "John");
        //When
        var act = () =>
        {
            LeaveRequestCreated.Create(
                Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f70"),
                DateTimeOffset.Parse("2023-12-19"),
                DateTimeOffset.Parse("2023-12-19"),
                TimeSpan.FromHours(8),
                Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f71"),
                "fake remarks",
                fakeUser,
                TimeSpan.FromHours(8)
            );
        };
        //Then
        act.Should().Throw<ArgumentException>().WithMessage("Input createdBy.Email was not in required format (Parameter 'createdBy.Email')");
    }

    [Fact]
    public void WhenEmailIsNull_ThenThrowArgumentNullException()
    {
        //Given
        var fakeUser = FederatedUser.Create("1", null, "John");
        //When
        var act = () =>
        {
            LeaveRequestCreated.Create(
                Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f72"),
                DateTimeOffset.Parse("2023-12-20"),
                DateTimeOffset.Parse("2023-12-20"),
                TimeSpan.FromHours(8),
                Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f73"),
                "fake remarks",
                fakeUser,
                TimeSpan.FromHours(8)
            );
        };
        //Then
        act.Should().Throw<ArgumentException>().WithMessage("Value cannot be null. (Parameter 'createdBy.Email')");
    }

    [Theory]
    [InlineData(-60)] // -1h
    [InlineData(0)] // 0h
    [InlineData(59)] // 59min
    [InlineData(24 * 60 + 1)] //1d 1m
    [InlineData(48 * 60)] // 2d
    public void WhenWorkingHoursIsInvalid_ThenThrowArgumentOutOfRangeException(int workingHoursInMinutes)
    {
        //Given
        var workingHours = TimeSpan.FromMinutes(workingHoursInMinutes);
        //When
        var act = () =>
        {
            LeaveRequestCreated.Create(
                Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f74"),
                DateTimeOffset.Parse("2023-12-21"),
                DateTimeOffset.Parse("2023-12-21"),
                TimeSpan.FromHours(8),
                Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f75"),
                "fake remarks",
                FederatedUser.Create("1", "good.email@fake.com", "John"),
                workingHours
            );
        };
        //Then
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Input workingHours was out of range (Parameter 'workingHours')");
    }

    [Theory]
    [InlineData(60)] // 1h
    [InlineData(8 * 60)] //8h
    [InlineData(24 * 60)] // 1h
    public void WhenWorkingHoursIsValid_ThenCreateObject(int workingHoursInMinutes)
    {
        //Given
        var workingHours = TimeSpan.FromMinutes(workingHoursInMinutes);
        var createdBy = FederatedUser.Create("1", "good.email@fake.com", "John");

        //When
        var result = LeaveRequestCreated.Create(
            Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f76"),
            DateTimeOffset.Parse("2023-12-22T12:57:06.5608725+00:00"),
            DateTimeOffset.Parse("2023-12-23T12:57:06.5608725+00:00"),
            TimeSpan.FromHours(8),
            Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f77"),
            "fake remarks",
            createdBy,
            workingHours
        );

        //Then
        result.CreatedBy.Should().Be(createdBy);
        result.DateFrom.Should().Be(DateTimeOffset.Parse("2023-12-22T00:00:00.00+00:00"));
        result.DateTo.Should().Be(DateTimeOffset.Parse("2023-12-23T00:00:00.00+00:00"));
        result.Duration.Should().Be(TimeSpan.FromHours(8));
        result.LeaveRequestId.Should().Be(Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f76"));
        result.LeaveTypeId.Should().Be(Guid.Parse("0e7e8dc7-d5b7-4e79-8897-10029c410f77"));
        result.Remarks.Should().Be("fake remarks");
        result.WorkingHours.Should().Be(workingHours);
    }
}
