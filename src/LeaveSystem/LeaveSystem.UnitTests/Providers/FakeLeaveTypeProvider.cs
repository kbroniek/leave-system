using System;
using System.Collections.Generic;
using System.Linq;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeLeaveTypeProvider
{
    private static readonly TimeSpan WorkingHours = TimeSpan.FromHours(8);
    public static Guid FakeOnDemandLeaveId = Guid.Parse("23288f39-4511-4476-b8ac-ff00176f0921");
    public static Guid FakeSickLeaveId = Guid.Parse("23288f39-4511-4476-b8ac-ff00176f0922");
    public static Guid FakeHolidayLeaveGuid = Guid.Parse("23288f39-4511-4476-b8ac-ff00176f0923");
    
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
        BaseLeaveTypeId = FakeHolidayLeaveGuid,
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
    public static IQueryable<LeaveType> GetLeaveTypes()
    {
        return new List<LeaveType> {
            GetFakeOnDemandLeave(),
            GetFakeHolidayLeave(),
            GetFakeSickLeave()
        }.AsQueryable();
    }
}