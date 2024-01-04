using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.Web.Pages.LeaveTypes;

namespace LeaveSystem.Web.UnitTests.TestStuff.Providers;

public class FakeLeaveTypeDtoProvider
{
    public static readonly Guid HolidayLeaveId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid;
    public static readonly Guid OnDemandLeaveId = FakeLeaveTypeProvider.FakeOnDemandLeaveId;
    public static readonly Guid SickLeaveId = FakeLeaveTypeProvider.FakeSickLeaveId;

    public static LeaveTypesService.LeaveTypeDto GetHolidayLeave() =>
        CreateDto(FakeLeaveTypeProvider.GetFakeHolidayLeave());

    public static LeaveTypesService.LeaveTypeDto GetOnDemandLeave() =>
        CreateDto(FakeLeaveTypeProvider.GetFakeOnDemandLeave());
    public static IEnumerable<LeaveTypesService.LeaveTypeDto> GetAll() =>
        FakeLeaveTypeProvider.GetLeaveTypes().Select(CreateDto);

    public static LeaveTypesService.LeaveTypeDto GetSickLeaveV2() =>
        new(SickLeaveId, OnDemandLeaveId, "fake sicke leave",
            new LeaveTypesService.LeaveTypeProperties(
                "fake color", LeaveTypeCatalog.Sick, false));

    public static LeaveTypesService.LeaveTypeDto GetOnDemandLeaveV2() =>
        new(OnDemandLeaveId, null, "fake sicke leave",
            new LeaveTypesService.LeaveTypeProperties(
                "fake color", LeaveTypeCatalog.OnDemand, true));

    public static IEnumerable<LeaveTypesService.LeaveTypeDto> GetAllV2() => new[]
        {
            GetSickLeaveV2(),
            GetOnDemandLeaveV2(),
            new(HolidayLeaveId, null, "fake leave",
                new LeaveTypesService.LeaveTypeProperties(
                    "fake color", LeaveTypeCatalog.Holiday, false)),
        };

    private static LeaveTypesService.LeaveTypeDto CreateDto(LeaveType leaveType) =>
        new(leaveType.Id, leaveType.BaseLeaveTypeId, leaveType.Name,
            new LeaveTypesService.LeaveTypeProperties(
                leaveType.Properties?.Color, leaveType.Properties?.Catalog, leaveType.Properties?.IncludeFreeDays));
}
