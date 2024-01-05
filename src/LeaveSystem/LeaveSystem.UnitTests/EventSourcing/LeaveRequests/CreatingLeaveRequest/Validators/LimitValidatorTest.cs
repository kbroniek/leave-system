using FluentAssertions;
using LeaveSystem.Db.Entities;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class LimitValidatorTest
{
    private readonly Mock<LeaveLimitsService> leaveLimitsServiceMock = new(null!);
    private readonly Mock<UsedLeavesService> usedLeavesServiceMock = new(null!);
    private readonly Mock<ConnectedLeaveTypesService> connectedLeaveTypesServiceMock = new(null!);

    private static LeaveRequestCreated GetFakeEvent(TimeSpan duration) => LeaveRequestCreated.Create(
        Guid.Parse("f34262fb-53c0-46bd-a64f-66e6834d94f1"),
        DateTimeOffset.Parse("2023-12-15"),
        DateTimeOffset.Parse("2023-12-18"),
        duration,
        Guid.Parse("f34262fb-53c0-46bd-a64f-66e6834d94f2"),
        "fake remarks",
        FakeUserProvider.GetUserWithNameFakeoslav(),
        WorkingHoursUtils.DefaultWorkingHours
    );

    [Theory]
    [InlineData(24.1, 10, 11, null)]
    [InlineData(25, 10, 11, 0.0)]
    [InlineData(25, 10, 10, 1.0)]
    [InlineData(1, 11, 10, 1.0)]
    [InlineData(8, 30, 24, 6.0)]
    public async Task WhenLimitIsLessThanUsedHours_ThenThrowException(double eventHours, double usedDays, double limitDays, double? overdueLimitDays)
    {
        //Given
        var fakeEvent = GetFakeEvent(TimeSpan.FromHours(eventHours));
        SetupConnectedLeaveTypeIds(fakeEvent);
        SetupLimit(fakeEvent, limitDays, overdueLimitDays);
        SetupUsedLimitDuration(fakeEvent.LeaveTypeId, fakeEvent.CreatedBy.Id, usedDays);

        var sut = GetSut();
        //When
        var act = async () => { await sut.Validate(fakeEvent); };
        //Then
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("You don't have enough free days for this type of leave");
        VerifyAll();
        VerifyGetLimit(Times.Once());
    }

    [Theory]
    [InlineData(24, 10, 11, null)]
    [InlineData(24, 10, 11, 0.0)]
    [InlineData(24, 10, 10, 1.0)]
    [InlineData(8, 26, 24, 6.0)]
    public async Task WhenLimitIsGraterThanUsedHours_ThenDoNotThrowException(double eventHours, double usedDays, double limitDays, double? overdueLimitDays)
    {
        //Given
        var fakeEvent = GetFakeEvent(TimeSpan.FromHours(eventHours));
        SetupConnectedLeaveTypeIds(fakeEvent);
        SetupLimit(fakeEvent, limitDays, overdueLimitDays);
        SetupUsedLimitDuration(fakeEvent.LeaveTypeId, fakeEvent.CreatedBy.Id, usedDays);

        var sut = GetSut();
        //When
        await sut.Validate(fakeEvent);
        //Then
        VerifyAll();
        VerifyGetLimit(Times.Once());
    }

    [Theory]
    [InlineData(24.1, 10, 11, null)]
    [InlineData(25, 10, 11, 0.0)]
    [InlineData(25, 10, 10, 1.0)]
    [InlineData(1, 11, 10, 1.0)]
    [InlineData(8, 30, 24, 6.0)]
    public async Task WhenConnectedLimitIsLessThanUsedHours_ThenThrowException(double eventHours, double usedDays, double limitDays, double? overdueLimitDays)
    {
        //Given
        var fakeEvent = GetFakeEvent(TimeSpan.FromHours(eventHours));
        Guid baseLeaveTypeId = Guid.Parse("da14b679-0451-4fb5-8ee2-610e90247e96");
        SetupConnectedLeaveTypeIds(fakeEvent, baseLeaveTypeId);
        SetupLimit(fakeEvent, 100, null);
        SetupLimit(fakeEvent, limitDays, overdueLimitDays, baseLeaveTypeId);
        SetupUsedLimitDuration(fakeEvent.LeaveTypeId, fakeEvent.CreatedBy.Id, 0);
        SetupUsedLimitDuration(baseLeaveTypeId, fakeEvent.CreatedBy.Id, usedDays);

        var sut = GetSut();
        //When
        var act = async () => { await sut.Validate(fakeEvent); };
        //Then
        await act.Should()
            .ThrowAsync<ValidationException>()
            .WithMessage("You don't have enough free days for this type of leave");
        VerifyAll();
        VerifyGetLimit(Times.Exactly(2));
    }

    [Theory]
    [InlineData(24, 10, 11, null)]
    [InlineData(24, 10, 11, 0.0)]
    [InlineData(24, 10, 10, 1.0)]
    [InlineData(8, 26, 24, 6.0)]
    public async Task WhenConnectedLimitIsGraterThanUsedHours_ThenDoNotThrowException(double eventHours, double usedDays, double limitDays, double? overdueLimitDays)
    {
        //Given
        var fakeEvent = GetFakeEvent(TimeSpan.FromHours(eventHours));
        Guid baseLeaveTypeId = Guid.Parse("da14b679-0451-4fb5-8ee2-610e90247e96");
        SetupConnectedLeaveTypeIds(fakeEvent, baseLeaveTypeId);
        SetupLimit(fakeEvent, 100, null);
        SetupLimit(fakeEvent, limitDays, overdueLimitDays, baseLeaveTypeId);
        SetupUsedLimitDuration(fakeEvent.LeaveTypeId, fakeEvent.CreatedBy.Id, 0);
        SetupUsedLimitDuration(baseLeaveTypeId, fakeEvent.CreatedBy.Id, usedDays);

        var sut = GetSut();
        //When
        var act = async () => { await sut.Validate(fakeEvent); };
        //Then
        await sut.Validate(fakeEvent);
        //Then
        VerifyAll();
        VerifyGetLimit(Times.Exactly(2));
    }

    private void VerifyGetLimit(Times times)
    {
        leaveLimitsServiceMock
                    .Verify(x => x.GetLimit(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<Guid>(), It.IsAny<string>()), times);
    }

    private void SetupConnectedLeaveTypeIds(LeaveRequestCreated fakeEvent, Guid? baseLeaveTypeId = null)
    {
        connectedLeaveTypesServiceMock
            .Setup(x => x.GetConnectedLeaveTypeIds(fakeEvent.LeaveTypeId))
            .ReturnsAsync(() => (baseLeaveTypeId, Enumerable.Empty<Guid>()))
            .Verifiable();
    }


    private void SetupLimit(LeaveRequestCreated fakeEvent, double limitDays, double? overdueLimitDays, Guid? leaveTypeId = null)
    {
        leaveLimitsServiceMock
            .Setup(x => x.GetLimit(fakeEvent.DateFrom, fakeEvent.DateTo, leaveTypeId ?? fakeEvent.LeaveTypeId, fakeEvent.CreatedBy.Id))
            .ReturnsAsync(() => new UserLeaveLimit
            {
                Limit = TimeSpan.FromDays(limitDays),
                OverdueLimit = overdueLimitDays is null ? null : TimeSpan.FromDays(overdueLimitDays.Value),
            });
    }
    private void SetupUsedLimitDuration(Guid leaveTypeId, string userId, double usedDays)
    {
        usedLeavesServiceMock
            .Setup(x => x.GetUsedLeavesDuration(
                DateTimeOffset.Parse("2023-01-01").GetFirstDayOfYear(),
                DateTimeOffset.Parse("2023-12-31").GetLastDayOfYear(),
                userId,
                leaveTypeId,
                It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(() => TimeSpan.FromDays(usedDays));
    }

    private void VerifyAll()
    {
        connectedLeaveTypesServiceMock.VerifyAll();
        leaveLimitsServiceMock.VerifyAll();
        usedLeavesServiceMock.VerifyAll();
    }

    private LimitValidator GetSut() =>
        new(leaveLimitsServiceMock.Object, usedLeavesServiceMock.Object, connectedLeaveTypesServiceMock.Object);
}
