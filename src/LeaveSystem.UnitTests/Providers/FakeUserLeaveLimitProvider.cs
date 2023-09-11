using System;
using System.Collections.Generic;
using LeaveSystem.Db.Entities;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeUserLeaveLimitProvider
{
    public static readonly Guid FakeLimitForSickLeaveId = Guid.NewGuid();
    public static readonly Guid FakeLimitForOnDemandLeaveId = Guid.NewGuid();
    public static readonly Guid FakeLimitForHolidayLeaveId = Guid.NewGuid();

    public static UserLeaveLimit GetLimitForSickLeave()
    {
        var now = DateTimeOffset.Now;
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
        var now = DateTimeOffset.Now;
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
        var now = DateTimeOffset.Now;
        return new UserLeaveLimit
        {
            Id = FakeLimitForHolidayLeaveId,
            LeaveTypeId = FakeLeaveTypeProvider.FakeHolidayLeaveGuid,
            Limit = FakeLeaveTypeProvider.GetFakeSickLeave().Properties?.DefaultLimit,
            AssignedToUserId = FakeUserProvider.GetUserWithNameFakeoslav().Id,
            ValidSince = now.GetFirstDayOfYear(),
            ValidUntil = now.GetLastDayOfYear(),
        };
    }

    public static IEnumerable<UserLeaveLimit> GetLimits()
    {
        yield return GetLimitForSickLeave();
        yield return GetLimitForHolidayLeave();
        yield return GetLimitForOnDemandLeave();
    }
}