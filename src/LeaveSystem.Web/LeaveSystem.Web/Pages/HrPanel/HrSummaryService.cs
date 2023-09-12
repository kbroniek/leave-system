using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.Web.Extensions;
using LeaveSystem.Web.Pages.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Web.Pages.LeaveRequests.ShowingLeaveRequests;
using LeaveSystem.Web.Pages.LeaveTypes;
using LeaveSystem.Web.Pages.UserLeaveLimits;
using LeaveSystem.Web.Pages.UserPanel;
using LeaveSystem.Web.Pages.WorkingHours;
using LeaveSystem.Web.Pages.WorkingHours.ShowingWorkingHours;

namespace LeaveSystem.Web.Pages.HrPanel;

public class HrSummaryService
{
    private readonly GetLeaveRequestsService getLeaveRequestsService;
    private readonly LeaveTypesService leaveTypesService;
    private readonly WorkingHoursService workingHoursService;
    private readonly UserLeaveLimitsService userLeaveLimitsService;
    private readonly EmployeeService employeeService;

    public HrSummaryService(
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
        var getLimitsTask = userLeaveLimitsService.GetLimits(firstDay, lastDay);
        var getEmployeesTask = employeeService.Get();
        await Task.WhenAll(getLeaveRequestsTask, getLeaveTypesTask, getLimitsTask, getEmployeesTask);
        var leaveRequests = getLeaveRequestsTask.Result?.Items ?? Enumerable.Empty<LeaveRequestShortInfo>();
        var leaveTypes = getLeaveTypesTask.Result ?? Enumerable.Empty<LeaveTypesService.LeaveTypeDto>();
        var limits = getLimitsTask.Result ?? Enumerable.Empty<UserLeaveLimitsService.LeaveLimitDto>();
        var employees = getEmployeesTask.Result
            .Union(leaveRequests.Select(lr =>
                GetEmployeeDto.Create(lr.CreatedBy)
            ));
        var allUserIds = limits
            .Select(l => l.AssignedToUserId)
            .Union(employees.Select(e => e.Id))
            .Distinct()
            .ToArray();
        var getWorkingHoursQuery = GetWorkingHoursQuery.GetDefaultForUsers(allUserIds);
        var workingHours = allUserIds.Length == 0 ?
            Enumerable.Empty<EventSourcing.WorkingHours.WorkingHours>() : 
            (await workingHoursService.GetWorkingHours(getWorkingHoursQuery))?.Items;

        return new LeaveRequestSummary(
            employees
                .Select(e => new UserLeaveRequestSummary(e,
                    leaveTypes.Select(lt => LeaveRequestPerType.Create(
                        lt,
                        leaveTypes,
                        leaveRequests.Where(lr => lr.CreatedBy.Id == e.Id),
                        limits.Where(l => l.AssignedToUserId == e.Id).Select(l => UserLeaveLimitsService.UserLeaveLimitDto.Create(l)),
                        workingHours.DurationOrDefault(e.Id))))),
        leaveTypes);
    }

    public sealed record LeaveRequestSummary(IEnumerable<UserLeaveRequestSummary> Items, IEnumerable<LeaveTypesService.LeaveTypeDto> LeaveTypes);
    public sealed record UserLeaveRequestSummary(GetEmployeeDto Employee, IEnumerable<LeaveRequestPerType> Summary);
}
