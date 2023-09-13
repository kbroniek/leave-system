using System;
using System.Collections.Generic;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeLeaveTypeProvider
{
    private static readonly TimeSpan WorkingHours = TimeSpan.FromHours(8);
    public static Guid FakeOnDemandLeaveId = Guid.NewGuid();
    public static Guid FakeSickLeaveId = Guid.NewGuid();
    public static Guid FakeHolidayLeaveGuid = Guid.NewGuid();
    
    public static LeaveType GetFakeSickLeave() => new()
    {
        Id = FakeSickLeaveId,
        Name = "niezdolność do pracy z powodu choroby",
        Order = 3,
        Properties = new LeaveType.LeaveTypeProperties
        {
            DefaultLimit = WorkingHours * 5,
            IncludeFreeDays = true,
            Color = "red",
            Catalog = LeaveTypeCatalog.Sick,
        }
    };

    public  static LeaveType GetFakeHolidayLeave() => new()
    {
        Id = FakeHolidayLeaveGuid,
        Name = "urlop wypoczynkowy",
        Order = 1,
        BaseLeaveTypeId = FakeOnDemandLeaveId,
        Properties = new LeaveType.LeaveTypeProperties
        {
            DefaultLimit = WorkingHours * 26,
            IncludeFreeDays = false,
            Color = "blue",
            Catalog = LeaveTypeCatalog.Holiday,
        }
    };

    public static LeaveType GetFakeOnDemandLeave() => new()
    {
        Id = FakeOnDemandLeaveId,
        Name = "urlop na żądanie",
        Order = 2,
        Properties = new LeaveType.LeaveTypeProperties
        {
            DefaultLimit = WorkingHours * 4,
            IncludeFreeDays = false,
            Color = "yellow",
            Catalog = LeaveTypeCatalog.OnDemand,
        }
    };
    
    public static LeaveType GetFakeWrongLeave() => new()
    {
        Id = Guid.Empty,
        Name = "niezdolność do pracy z powodu choroby",
        Order = 3
    };

    public static IEnumerable<LeaveType> GetLeaveTypes()
    {
        yield return GetFakeOnDemandLeave();
        yield return GetFakeHolidayLeave();
        yield return GetFakeSickLeave();
    }
}