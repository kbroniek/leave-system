using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.UserPanel;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;

namespace LeaveSystem.Web.UnitTests.Pages.UserPanel;

public class CreateForViewTest
{
    [Fact]
    public void WhenCreatingForView_ThenCreateWithFirstMatchingLimit()
    {
        //Given
        var exceptedLimit = new UserLeaveLimitsService.UserLeaveLimitDto(TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6),
            DateTimeOffsetExtensions.CreateFromDate(2023, 9, 3),
            new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fake desc3"));
        var limits = new[]
        {
            new(TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2022, 1, 3),
                DateTimeOffsetExtensions.CreateFromDate(2022, 5, 6),
                new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fake desc1")),
            new(TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2022, 11, 3),
                DateTimeOffsetExtensions.CreateFromDate(2022, 12, 3),
                new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fake desc2")),
            exceptedLimit,
            new(TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2023, 12, 23),
                DateTimeOffsetExtensions.CreateFromDate(2024, 6, 22),
                new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fake desc4"))
        };
        var leaveRequest =
            new LeaveRequestShortInfo(Guid.NewGuid(), DateTimeOffsetExtensions.CreateFromDate(2023, 6, 7),
                DateTimeOffsetExtensions.CreateFromDate(2023, 9, 2), TimeSpan.FromHours(16),
                FakeLeaveTypeDtoProvider.HolidayLeaveId,
                LeaveRequestStatus.Accepted, FakeUserProvider.GetUserBen());
        var workingHours = TimeSpan.FromHours(8);
        //When
        var result = LeaveRequestPerType.ForView.CreateForView(leaveRequest, limits, workingHours);
        //Then
        result.Should().BeEquivalentTo(new
        {
            Id = leaveRequest.Id,
            DateFrom = leaveRequest.DateFrom,
            DateTo = leaveRequest.DateTo,
            Duration = leaveRequest.Duration.GetReadableTimeSpan(workingHours),
            Description = exceptedLimit.Property.Description,
            Status = leaveRequest.Status
        });
    }

    [Fact]
    public void WhenNoLimitInLeaveRequestPeriod_ThenDescriptionIsNull()
    {
        //Given
        var exceptedLimit = new UserLeaveLimitsService.UserLeaveLimitDto(TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6),
            DateTimeOffsetExtensions.CreateFromDate(2023, 9, 3),
            new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fake desc3"));
        var limits = new[]
        {
            new(TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2022, 1, 3),
                DateTimeOffsetExtensions.CreateFromDate(2022, 5, 6),
                new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fake desc1")),
            new(TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2022, 11, 3),
                DateTimeOffsetExtensions.CreateFromDate(2022, 12, 3),
                new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fake desc2")),
            exceptedLimit,
            new(TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2023, 12, 23),
                DateTimeOffsetExtensions.CreateFromDate(2024, 6, 22),
                new UserLeaveLimitsService.UserLeaveLimitPropertyDto("fake desc4"))
        };
        var leaveRequest =
            new LeaveRequestShortInfo(Guid.NewGuid(), DateTimeOffsetExtensions.CreateFromDate(2023, 6, 7),
                DateTimeOffsetExtensions.CreateFromDate(2024, 10, 2), TimeSpan.FromHours(16),
                FakeLeaveTypeDtoProvider.HolidayLeaveId,
                LeaveRequestStatus.Accepted, FakeUserProvider.GetUserBen());
        var workingHours = TimeSpan.FromHours(8);
        //When
        var result = LeaveRequestPerType.ForView.CreateForView(leaveRequest, limits, workingHours);
        //Then
        result.Should().BeEquivalentTo(new
        {
            Description = (string?)null,
        }, o => o.ExcludingMissingMembers());
    }
}