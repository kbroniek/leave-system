using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest.Validators;

public class CreateLeaveRequestValidatorTest
{
    private readonly LeaveRequestCreated fakeEvent = LeaveRequestCreated.Create(
        Guid.Parse("3ace29bf-4dce-44f4-90b4-10f3ed70c80e"),
        DateTimeOffset.Parse("2023-12-15"),
        DateTimeOffset.Parse("2023-12-18"),
        TimeSpan.FromHours(8),
        Guid.Parse("3ace29bf-4dce-44f4-90b4-10f3ed70c801"),
        "fake remarks",
        FakeUserProvider.GetUserWithNameFakeoslav(),
        WorkingHoursUtils.DefaultWorkingHours
    );
    [Fact]
    public async Task WhenLimitIsLessThanUsedHours_ThenThrowException()
    {
        //Given
        var minDuration = TimeSpan.FromHours(1);
        var maxDuration = TimeSpan.FromHours(8);
        bool? includeFreeDays = true;
        var basicValidatorMock = new Mock<BasicValidator>(null!);
        var impositionValidatorMock = new Mock<ImpositionValidator>(null!);
        var limitValidatorMock = new Mock<LimitValidator>(null!, null!, null!);

        var sut = new CreateLeaveRequestValidator(basicValidatorMock.Object, impositionValidatorMock.Object, limitValidatorMock.Object);
        //When
        await sut.Validate(fakeEvent, minDuration, maxDuration, includeFreeDays);
        //Then
        basicValidatorMock.Verify(x => x.DataRangeValidate(fakeEvent), Times.Once);
        basicValidatorMock.Verify(x => x.Validate(fakeEvent, minDuration, maxDuration, includeFreeDays), Times.Once);
        impositionValidatorMock.Verify(x => x.Validate(fakeEvent), Times.Once);
        limitValidatorMock.Verify(x => x.Validate(fakeEvent), Times.Once);
    }
}
