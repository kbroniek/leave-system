using LeaveSystem.Shared;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.UserPanel;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;

namespace LeaveSystem.Web.UnitTests.Pages.UserPanel;

public class CreateLeaveRequestPerTypeTest
{
    [Theory]
    [MemberData(nameof(Get_WhenProvidingArguments_ThenCreateCorrectLeaveRequestPerType_TestData))]
    public void WhenProvidingArguments_ThenCreateCorrectLeaveRequestPerType(
        LeaveTypesService.LeaveTypeDto leaveType,
        IEnumerable<LeaveTypesService.LeaveTypeDto> allLeaveTypes,
        IEnumerable<LeaveRequestShortInfo> leaveRequests,
        IEnumerable<UserLeaveLimitDto> limits,
        TimeSpan workingHours,
        TimeSpan leaveRequestsUsed,
        TimeSpan limitsSum,
        TimeSpan overdueLimitSum,
        TimeSpan totalLimit,
        TimeSpan left,
        IEnumerable<LeaveRequestPerType.ForView> leaveRequestsWithDescription)
    {
        //Given
        //When
        var result = LeaveRequestPerType.Create(leaveType, allLeaveTypes, leaveRequests, limits);
        //Then
        result.Should().BeEquivalentTo(new
        {
            LeaveTypeName = leaveType.Name,
            LeaveTypeId = leaveType.Id,
            Used = leaveRequestsUsed.GetReadableTimeSpan(workingHours),
            Limit = limitsSum.GetReadableTimeSpan(workingHours),
            OverdueLimit = overdueLimitSum.GetReadableTimeSpan(workingHours),
            SumLimit = totalLimit.GetReadableTimeSpan(workingHours),
            Left = left.GetReadableTimeSpan(workingHours),
            LeaveRequests = leaveRequestsWithDescription,
            LeaveTypeProperties = leaveType.Properties
        });
    }

    public static IEnumerable<object[]> Get_WhenProvidingArguments_ThenCreateCorrectLeaveRequestPerType_TestData()
    {
        var now = DateTimeOffset.Now;
        var firstFakeWorkingHours = TimeSpan.FromHours(8);
        var firstFakeLeaveRequests = FakeLeaveRequestShortInfoProvider.GetAll(now, firstFakeWorkingHours);
        var firstFakeLimits = FakeUserLeaveLimitsDtoProvider.GetAllUserLimits(now.Year);
        yield return new object[]
        {
            FakeLeaveTypeDtoProvider.GetHolidayLeave(),
            FakeLeaveTypeDtoProvider.GetAll(),
            firstFakeLeaveRequests,
            firstFakeLimits,
            firstFakeWorkingHours,
            TimeSpan.FromHours(24),
            TimeSpan.FromHours(8),
            TimeSpan.FromHours(16),
            TimeSpan.FromHours(24),
            TimeSpan.FromHours(0),
            firstFakeLeaveRequests.Select(lr => LeaveRequestPerType.ForView.CreateForView(lr, firstFakeLimits, firstFakeWorkingHours))
        };
        var secondFakeWorkingHours = TimeSpan.FromHours(4);
        var secondFakeLeaveRequests = FakeLeaveRequestShortInfoProvider.GetAllV2(now, secondFakeWorkingHours);
        var secondFakeLimits = FakeUserLeaveLimitsDtoProvider.GetAllUserLimits(now.Year);
        yield return new object[]
        {
            FakeLeaveTypeDtoProvider.GetOnDemandLeaveV2(),
            FakeLeaveTypeDtoProvider.GetAllV2(),
            secondFakeLeaveRequests,
            secondFakeLimits,
            secondFakeWorkingHours,
            TimeSpan.FromHours(20),
            TimeSpan.FromHours(32),
            TimeSpan.FromHours(40),
            TimeSpan.FromHours(72),
            TimeSpan.FromHours(52),
            secondFakeLeaveRequests.Select(lr =>
                LeaveRequestPerType.ForView.CreateForView(lr, secondFakeLimits, secondFakeWorkingHours))
        };
    }
    [Fact]
    public void WhenNoLimitsPerLeaveType_ThenLimitsReturnLimitOverdueLimitSumLimitAndLeftAsNull()
    {
        //Given
        var now = DateTimeOffset.Now;
        LeaveTypesService.LeaveTypeDto leaveType = new(Guid.NewGuid(), null, "fake name",
            new LeaveTypesService.LeaveTypeProperties("fc", LeaveTypeCatalog.Holiday, null));
        var allLeaveTypes = FakeLeaveTypeDtoProvider.GetAll();
        var leaveRequests = FakeLeaveRequestShortInfoProvider.GetAll(now);
        var limits = FakeUserLeaveLimitsDtoProvider.GetAllUserLimits(now.Year);
        //When
        var result = LeaveRequestPerType.Create(leaveType, allLeaveTypes, leaveRequests, limits);
        //Then
        result.Should().BeEquivalentTo(new
        {
            Limit = (string?)null,
            OverdueLimit = (string?)null,
            SumLimit = (string?)null,
            Left = (string?)null,
        }, o => o.ExcludingMissingMembers());
    }
}
