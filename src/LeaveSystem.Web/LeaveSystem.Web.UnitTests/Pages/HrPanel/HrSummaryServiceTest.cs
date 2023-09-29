using Blazored.Toast.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Extensions;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.HrPanel;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.UserPanel;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;
using LeaveSystem.Web.UnitTests.TestStuff.Extensions;
using LeaveSystem.Web.UnitTests.TestStuff.Providers;

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
        var query = new GetLeaveRequestsQuery(firstDay, lastDay, 1, 1000);
        var fakeLeaveRequests =
            FakeLeaveRequestShortInfoProvider.GetAll(DateTimeOffsetExtensions.CreateFromDate(year, 4, 5))
                .ToPagedListResponse();
        getLeaveRequestsServiceMock.GetLeaveRequests(ArgExtensions.IsEquivalentTo<GetLeaveRequestsQuery>(query))
            .Returns(fakeLeaveRequests);
        var leaveTypesServiceMock = Substitute.For<LeaveTypesService>(httpClientMock);
        var fakeLeaveTypes = FakeLeaveTypeDtoProvider.GetAll();
        leaveTypesServiceMock.GetLeaveTypes().Returns(fakeLeaveTypes);
        var userLeaveLimitsServiceMock = Substitute.For<UserLeaveLimitsService>(httpClientMock);
        var fakeLimits = FakeLeaveLimitsDtoProvider.GetAll(year);
        userLeaveLimitsServiceMock.GetLimits(firstDay, lastDay).Returns(fakeLimits);
        var employeeServiceMock = Substitute.For<EmployeeService>(httpClientMock);
        var employees = FakeGetEmployeesDtoProvider.GetAll().ToArray();
        employeeServiceMock.Get().Returns(employees);
        var workingHoursServiceMock = Substitute.For<WorkingHoursService>(httpClientMock, Substitute.For<IToastService>());
        var fakeUserIds = new[] { FakeUserProvider.BenId, FakeUserProvider.PhilipId, FakeUserProvider.HabibId, FakeUserProvider.FakseoslavId};
        var fakeWorkingHours = FakeWorkingHoursProvider.GetAll(DateTimeOffset.Now).ToDto().ToPagedListResponse();
        var getWorkingHoursQuery = GetWorkingHoursQuery.GetDefaultForUsers(fakeUserIds);
        workingHoursServiceMock.GetWorkingHours(ArgExtensions.IsEquivalentTo<GetWorkingHoursQuery>(getWorkingHoursQuery))
            .Returns(fakeWorkingHours);
        var sut = GetSut(getLeaveRequestsServiceMock, leaveTypesServiceMock, workingHoursServiceMock,
            userLeaveLimitsServiceMock, employeeServiceMock);
        //When
        var result = await sut.Summary(year);
        //Then
        await getLeaveRequestsServiceMock.Received().GetLeaveRequests(ArgExtensions.IsEquivalentTo<GetLeaveRequestsQuery>(query));
        await leaveTypesServiceMock.Received().GetLeaveTypes();
        await userLeaveLimitsServiceMock.Received().GetLimits(firstDay, lastDay);
        await employeeServiceMock.Received().Get();
        await workingHoursServiceMock.Received().GetWorkingHours(ArgExtensions.IsEquivalentTo<GetWorkingHoursQuery>(getWorkingHoursQuery));
        var items = employees.Union(fakeLeaveRequests.Items.Select(lr =>
            GetEmployeeDto.Create(lr.CreatedBy)
        )).Select(e =>
            new HrSummaryService.UserLeaveRequestSummary(e, fakeLeaveTypes.Select(lt => LeaveRequestPerType.Create(
                lt,
                fakeLeaveTypes,
                fakeLeaveRequests.Items.Where(lr => lr.CreatedBy.Id == e.Id),
                fakeLimits.Where(l => l.AssignedToUserId == e.Id).Select(l => UserLeaveLimitsService.UserLeaveLimitDto.Create(l)),
                fakeWorkingHours.Items.DurationOrZero(e.Id))))
        );
        result.Should().BeEquivalentTo(new
        {
            LeaveTypes = fakeLeaveTypes,
            Items = items
        }, o => o.ExcludingMissingMembers());
    }

}