using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.LeaveTypes;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public class FakeLeaveTypeDtoProvider
{
    public static Guid HolidayLeaveId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid;
    public static Guid OnDemandLeaveId = FakeLeaveTypeProvider.FakeOnDemandLeaveId;
    public static Guid SickLeaveId = FakeLeaveTypeProvider.FakeSickLeaveId;

    public static IEnumerable<LeaveTypesService.LeaveTypeDto> GetAll()
    {
        return FakeLeaveTypeProvider.GetLeaveTypes().Select(leaveType => new LeaveTypesService.LeaveTypeDto(leaveType.Id, leaveType.BaseLeaveTypeId, leaveType.Name,
            new LeaveTypesService.LeaveTypeProperties(
                leaveType.Properties.Color, leaveType.Properties.Catalog, leaveType.Properties.IncludeFreeDays)
        ));
    }
}