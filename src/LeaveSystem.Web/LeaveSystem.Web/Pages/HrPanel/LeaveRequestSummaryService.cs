using LeaveSystem.Shared;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.UserPanel;
using LeaveSystem.Web.Pages.WorkingHours;

namespace LeaveSystem.Web.Pages.HrPanel;

public class LeaveRequestSummaryService
{
    private readonly GetLeaveRequestsService getLeaveRequestsService;
    private readonly LeaveTypesService leaveTypesService;
    private readonly WorkingHoursService workingHoursService;
    private readonly UserLeaveLimitsService userLeaveLimitsService;
    private readonly EmployeeService employeeService;

    public LeaveRequestSummaryService(
        GetLeaveRequestsService getLeaveRequestsService,
        LeaveTypesService leaveTypesService,
        WorkingHoursService workingHoursService,
        UserLeaveLimitsService userLeaveLimitsService,
        EmployeeService employeeService)
    {
        this.getLeaveRequestsService = getLeaveRequestsService;
        this.leaveTypesService = leaveTypesService;
        this.workingHoursService = workingHoursService;
        this.userLeaveLimitsService = userLeaveLimitsService;
        this.employeeService = employeeService;
    }
    public async Task<LeaveRequestSummary> Summary(int year)
    {
        var firstDay = DateTimeOffsetExtensions.GetFirstDayOfYear(year);
        var lastDay = DateTimeOffsetExtensions.GetLastDayOfYear(year);
        var query = new GetLeaveRequestsQuery(firstDay, lastDay, 1, 1000);
        var getLeaveRequestsTask = getLeaveRequestsService.GetLeaveRequests(query);
        var getLeaveTypesTask = leaveTypesService.GetLeaveTypes();
        var getLimitsTask = userLeaveLimitsService.GetLimits();
        var getEmployeesTask = employeeService.Get();
        await Task.WhenAll(getLeaveRequestsTask, getLeaveTypesTask, getLimitsTask, getEmployeesTask);
        var leaveRequests = getLeaveRequestsTask.Result?.Items ?? Enumerable.Empty<LeaveRequestShortInfo>();
        var leaveTypes = getLeaveTypesTask.Result ?? Enumerable.Empty<LeaveTypesService.LeaveTypeDto>();
        var limits = getLimitsTask.Result ?? Enumerable.Empty<UserLeaveLimitsService.LeaveLimitDto>();
        var employees = getEmployeesTask.Result;
        var allUserIds = limits
            .Select(l => l.AssignedToUserId)
            .Union(leaveRequests.Select(lr => lr.CreatedBy.Id))
            .Distinct();
        var workingHours = await workingHoursService.GetWorkingHours(allUserIds, firstDay, lastDay);

        return new LeaveRequestSummary(
            employees
                .Select(e => new UserLeaveRequestSummary(e,
                    leaveTypes.Select(lt => LeaveRequestPerType.Create(
                        lt,
                        leaveRequests.Where(lr => lr.CreatedBy.Id == e.Id),
                        limits.Where(l => l.AssignedToUserId == e.Id).Select(l => UserLeaveLimitsService.UserLeaveLimitDto.Create(l)),
                        workingHours.GetDuration(e.Id))))),
        leaveTypes);
    }

    public sealed record LeaveRequestSummary(IEnumerable<UserLeaveRequestSummary> Items, IEnumerable<LeaveTypesService.LeaveTypeDto> LeaveTypes);
    public sealed record UserLeaveRequestSummary(GetEmployeeDto Employee, IEnumerable<LeaveRequestPerType> Summary);
}
