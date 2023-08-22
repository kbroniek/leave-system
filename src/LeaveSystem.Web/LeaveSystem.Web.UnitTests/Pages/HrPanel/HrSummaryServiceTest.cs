using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.HrPanel;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;
using Moq;
using NSubstitute;

namespace LeaveSystem.Web.UnitTests.Pages.HrPanel;

public class HrSummaryServiceTest
{
    private HrSummaryService GetSut(
        GetLeaveRequestsService getLeaveRequestsService,
        LeaveTypesService leaveTypesService,
        WorkingHoursService workingHoursService,
        UserLeaveLimitsService userLeaveLimitsService,
        EmployeeService employeeService)
    {
        return new HrSummaryService(getLeaveRequestsService, leaveTypesService, workingHoursService, userLeaveLimitsService, employeeService);
    }

    [Fact]
    public async Task WhenOperationIsSuccessful_ThenReturnSummary()
    {
        const int year = 2021;
        var firstDay = DateTimeOffsetExtensions.GetFirstDayOfYear(year);
        var lastDay = DateTimeOffsetExtensions.GetLastDayOfYear(year);
        var httpClientMock = Substitute.For<HttpClient>();
        var getLeaveRequestsServiceMock = Substitute.For<GetLeaveRequestsService>(httpClientMock);
        getLeaveRequestsServiceMock.GetLeaveRequests(It.IsAny<GetLeaveRequestsQuery>())
            .ReturnsAsync(FakeLeaveRequestShortInfoProvider.GetAll(new DateTimeOffset(year, 4, 5, 0,0,0,TimeSpan.Zero))
                .ToPagedListResponse(1000));
        var leaveTypesServiceMock = Substitute.For<LeaveTypesService>(httpClientMock);
        leaveTypesServiceMock.GetLeaveTypes().Returns(FakeLeaveTypeDtoProvider.GetAll());
        var userLeaveLimitsService = Substitute.For<UserLeaveLimitsService>(httpClientMock);
        userLeaveLimitsService.GetLimits(firstDay, lastDay).Returns(FakeUserLeaveLimitsDtoProvider.GetAll(year));
        var employeeServiceMock = Substitute.For<EmployeeService>(httpClientMock);
        employeeServiceMock.Get().Returns(FakeGetEmployeesDtoProvider.GetAll());
        var workingHoursServiceMock = Substitute.For<WorkingHoursService>(httpClientMock);
        workingHoursServiceMock.GetWorkingHours(FakeWorkingHoursProvider.GetUserIds(), firstDay, lastDay)
            .Returns(FakeWorkingHoursProvider.GetAllAsWorkingHoursCollection());

    } 
    
}