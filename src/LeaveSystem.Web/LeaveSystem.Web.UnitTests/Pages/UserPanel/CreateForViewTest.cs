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
        var exceptedLimit = new UserLeaveLimitDto(Guid.Parse("d2ca9535-8fd6-4e18-9465-4edf660e56b4"),TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6),
            DateTimeOffsetExtensions.CreateFromDate(2023, 9, 3),
            new UserLeaveLimitPropertyDto("fake desc3"));
        var limits = new[]
        {
            new(Guid.Parse("86bf1b71-b266-4810-9fc1-a63ac48f0749"),TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2022, 1, 3),
                DateTimeOffsetExtensions.CreateFromDate(2022, 5, 6),
                new UserLeaveLimitPropertyDto("fake desc1")),
            new(Guid.Parse("D163C40F-2E64-443D-9911-B448D5E04FEC"),TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2022, 11, 3),
                DateTimeOffsetExtensions.CreateFromDate(2022, 12, 3),
                new UserLeaveLimitPropertyDto("fake desc2")),
            exceptedLimit,
            new(Guid.Parse("1C3B6226-040E-4D8E-AF8B-BBABA5B2B445"),TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2023, 12, 23),
                DateTimeOffsetExtensions.CreateFromDate(2024, 6, 22),
                new UserLeaveLimitPropertyDto("fake desc4"))
        };
        var leaveRequest =
            new LeaveRequestShortInfo(Guid.Parse("443dbfc1-154b-454a-9925-70c795d0fb7b"), DateTimeOffsetExtensions.CreateFromDate(2023, 6, 7),
                DateTimeOffsetExtensions.CreateFromDate(2023, 9, 2), TimeSpan.FromHours(16),
                FakeLeaveTypeDtoProvider.HolidayLeaveId,
                LeaveRequestStatus.Accepted, FakeUserProvider.GetUserBen());
        var workingHours = TimeSpan.FromHours(8);
        //When
        var result = LeaveRequestPerType.ForView.CreateForView(leaveRequest, limits, workingHours);
        //Then
        result.Should().BeEquivalentTo(new
        {
            leaveRequest.Id,
            leaveRequest.DateFrom,
            leaveRequest.DateTo,
            Duration = leaveRequest.Duration.GetReadableTimeSpan(workingHours),
            exceptedLimit.Property?.Description,
            leaveRequest.Status
        });
    }

    [Fact]
    public void WhenNoLimitInLeaveRequestPeriod_ThenDescriptionIsNull()
    {
        //Given
        var exceptedLimit = new UserLeaveLimitDto(Guid.Parse("12c4c87e-1f3d-4b3b-adde-803df439e08a"),TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
            DateTimeOffsetExtensions.CreateFromDate(2023, 5, 6),
            DateTimeOffsetExtensions.CreateFromDate(2023, 9, 3),
            new UserLeaveLimitPropertyDto("fake desc3"));
        var limits = new[]
        {
            new(Guid.Parse("d6cc20e1-169c-4e67-acda-fc15ce40e4df"),TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2022, 1, 3),
                DateTimeOffsetExtensions.CreateFromDate(2022, 5, 6),
                new UserLeaveLimitPropertyDto("fake desc1")),
            new(Guid.Parse("b61e0b77-8b40-4882-9183-53bf9057d398"),TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2022, 11, 3),
                DateTimeOffsetExtensions.CreateFromDate(2022, 12, 3),
                new UserLeaveLimitPropertyDto("fake desc2")),
            exceptedLimit,
            new(Guid.Parse("358bcaba-6546-45b4-b766-77086e3f3a55"),TimeSpan.FromHours(8), TimeSpan.FromHours(8), FakeLeaveTypeDtoProvider.HolidayLeaveId,
                DateTimeOffsetExtensions.CreateFromDate(2023, 12, 23),
                DateTimeOffsetExtensions.CreateFromDate(2024, 6, 22),
                new UserLeaveLimitPropertyDto("fake desc4"))
        };
        var leaveRequest =
            new LeaveRequestShortInfo(Guid.Parse("c7cc99a6-f431-4340-82b0-7cd69dec5807"), DateTimeOffsetExtensions.CreateFromDate(2023, 6, 7),
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
