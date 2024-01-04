using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using System;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class BasicValidatorTest
{

    [Theory]
    [InlineData(0)] // Zero hours
    [InlineData(-1)] // Minus one hours
    public void WhenDurationIsZeroOrNegative_ThenThrowArgumentException(int hours)
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc083"),
            DateTimeOffset.Parse("2023-12-16"),
            DateTimeOffset.Parse("2023-12-16"),
            TimeSpan.FromHours(hours),
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc084"),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = new BasicValidator(FakeDateServiceProvider.GetDateService());
        //When
        var act = () =>
        {
            sut.Validate(
                leaveRequestCreated,
                TimeSpan.FromHours(1), // One days time range
                TimeSpan.FromHours(8),
                false);
        };
        //Then
        var resut = act.Should().Throw<ArgumentException>();
        resut.WithMessage("Required input Duration cannot be zero or negative. (Parameter 'Duration')");
    }

    [Theory]
    [InlineData(1)] // One hours
    [InlineData(8)] // One day
    [InlineData(17)] // Two days and one hour
    [InlineData(24)] // Three days
    public void WhenDurationTwoDaysIsOutOfRange_ThenThrowArgumentOutOfRangeException(int hours)
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc085"),
            DateTimeOffset.Parse("2023-12-17"),
            DateTimeOffset.Parse("2023-12-17"),
            TimeSpan.FromHours(hours),
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc086"),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = new BasicValidator(FakeDateServiceProvider.GetDateService());
        //When
        var act = () =>
        {
            sut.Validate(
                leaveRequestCreated,
                TimeSpan.FromHours(9), // Two days time range
                TimeSpan.FromHours(16),
                null);
        };
        //Then
        var resut = act.Should().Throw<ArgumentOutOfRangeException>();
        resut.WithMessage("Input Duration was out of range (Parameter 'Duration')");
    }

    [Theory]
    [InlineData("2023-12-17", "2023-12-18")] //DateFrom free day
    [InlineData("2023-12-15", "2023-12-16")] //DateTo free day
    [InlineData("2023-12-16", "2023-12-17")] //DateFrom and DateTo free days
    public void WhenDateFromDateToOrDurationIsWorkingDay_ThenThrowArgumentOutOfRangeException(
        string dateFrom, string dateTo
    )
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc081"),
            DateTimeOffset.Parse(dateFrom),
            DateTimeOffset.Parse(dateTo),
            TimeSpan.FromHours(16),
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc082"),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = new BasicValidator(FakeDateServiceProvider.GetDateService());
        //When
        var act = () =>
        {
            sut.Validate(
                leaveRequestCreated,
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(16),
                false);
        };
        //Then
        var result = act.Should().Throw<ArgumentOutOfRangeException>();
        result.WithMessage($"The date * is off work. (Parameter 'event')");
    }

    [Theory]
    [InlineData("2023-12-17", "2023-12-18", null)] //DateFrom free day
    [InlineData("2023-12-15", "2023-12-16", true)] //DateTo free day
    [InlineData("2023-12-16", "2023-12-17", true)] //DateFrom and DateTo free days
    [InlineData("2023-12-20", "2023-12-21", false)]
    public void WhenEventIsCorrect_ThenDoNothing(
        string dateFrom, string dateTo, bool? includeFreeDays
    )
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc083"),
            DateTimeOffset.Parse(dateFrom),
            DateTimeOffset.Parse(dateTo),
            TimeSpan.FromHours(16),
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc084"),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = new BasicValidator(FakeDateServiceProvider.GetDateService());
        //When Then
        sut.Validate(
                leaveRequestCreated,
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(16),
                includeFreeDays);
    }

    [Theory]
    [InlineData("2022-12-31", "2023-01-01", "DateFrom")] //DateFrom previous year
    [InlineData("2024-12-31", "2023-01-01", "DateFrom")] //DateFrom next year
    [InlineData("2023-12-31", "2022-01-01", "DateTo")] //DateTo previous year
    [InlineData("2023-12-31", "2024-01-01", "DateTo")] //DateTo next year
    public void WhenDateIsOutOfRange_ThenThrowArgumentOutOfRangeException(
        string dateFrom, string dateTo, string invalidParameterName
    )
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc085"),
            DateTimeOffset.Parse(dateFrom),
            DateTimeOffset.Parse(dateTo),
            TimeSpan.FromHours(16),
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc086"),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = new BasicValidator(FakeDateServiceProvider.GetDateService());
        //When
        var act = () =>
        {
            sut.DataRangeValidate(leaveRequestCreated);
        };
        //Then
        var result = act.Should().Throw<ArgumentOutOfRangeException>();
        result.WithMessage($"Input {invalidParameterName} was out of range (Parameter '{invalidParameterName}')");
    }

    [Theory]
    [InlineData("2023-01-02", "2023-01-01")]
    [InlineData("2023-12-31", "2023-01-01")]
    public void WhenDateFromIsGreaterThanDateTo_ThenThrowArgumentOutOfRangeException(
        string dateFrom, string dateTo
    )
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc087"),
            DateTimeOffset.Parse(dateFrom),
            DateTimeOffset.Parse(dateTo),
            TimeSpan.FromHours(16),
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc088"),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = new BasicValidator(FakeDateServiceProvider.GetDateService());
        //When
        var act = () =>
        {
            sut.DataRangeValidate(leaveRequestCreated);
        };
        //Then
        var result = act.Should().Throw<ArgumentOutOfRangeException>();
        result.WithMessage("Date from has to be less than date to. (Parameter 'event')");
    }

    [Theory]
    [InlineData("2023-01-01", "2023-01-01")]
    [InlineData("2023-12-31", "2023-12-31")]
    [InlineData("2023-06-16", "2023-06-17")]
    [InlineData("2023-01-01", "2023-12-31")]
    public void WhenDataRangeIsCorrect_ThenDoNothing(
        string dateFrom, string dateTo
    )
    {
        //Given
        var leaveRequestCreated = LeaveRequestCreated.Create(
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc089"),
            DateTimeOffset.Parse(dateFrom),
            DateTimeOffset.Parse(dateTo),
            TimeSpan.FromHours(16),
            Guid.Parse("79e2de8c-56fb-4fd1-92e1-f1f8ea6fc08a"),
            "fake remarks",
            FederatedUser.Create("1", "fakeUser@fake.com", "Fakeoslav"),
            WorkingHoursUtils.DefaultWorkingHours
        );
        var sut = new BasicValidator(FakeDateServiceProvider.GetDateService());
        //When
        //When Then
        sut.DataRangeValidate(leaveRequestCreated);
    }
}