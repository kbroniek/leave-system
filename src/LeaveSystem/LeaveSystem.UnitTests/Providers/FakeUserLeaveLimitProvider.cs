using System;
using System.Collections.Generic;
using System.Linq;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeUserLeaveLimitProvider
{
    public static readonly Guid FakeLimitForSickLeaveId = Guid.Parse("e12b8ce4-3e5c-4b1c-be7a-eb3276383261");
    public static readonly Guid FakeLimitForOnDemandLeaveId = Guid.Parse("e12b8ce4-3e5c-4b1c-be7a-eb3276383262");
    public static readonly Guid FakeLimitForHolidayLeaveId = Guid.Parse("e12b8ce4-3e5c-4b1c-be7a-eb3276383263");

    public static UserLeaveLimit GetLimitForSickLeave()
    {
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        return new UserLeaveLimit
        {
            Id = FakeLimitForSickLeaveId,
            LeaveTypeId = FakeLeaveTypeProvider.FakeSickLeaveId,
            Limit = FakeLeaveTypeProvider.GetFakeSickLeave().Properties?.DefaultLimit,
            AssignedToUserId = FakeUserProvider.GetUserWithNameFakeoslav().Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
    }
    public static UserLeaveLimit GetLimitForOnDemandLeave()
    {
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        return new UserLeaveLimit
        {
            Id = FakeLimitForOnDemandLeaveId,
            LeaveTypeId = FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            Limit = FakeLeaveTypeProvider.GetFakeSickLeave().Properties?.DefaultLimit,
            AssignedToUserId = FakeUserProvider.GetUserWithNameFakeoslav().Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
    }
    public static UserLeaveLimit GetLimitForHolidayLeave()
    {
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        return new UserLeaveLimit
        {
            Id = FakeLimitForHolidayLeaveId,
            LeaveTypeId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
            Limit = FakeLeaveTypeProvider.GetFakeHolidayLeave().Properties?.DefaultLimit,
            AssignedToUserId = FakeUserProvider.GetUserWithNameFakeoslav().Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
    }

    public static IQueryable<UserLeaveLimit> GetLimits()
    {
        return new List<UserLeaveLimit>
        {
            GetLimitForSickLeave(),
            GetLimitForHolidayLeave(),
            GetLimitForOnDemandLeave(),
        }.AsQueryable();
    }
}