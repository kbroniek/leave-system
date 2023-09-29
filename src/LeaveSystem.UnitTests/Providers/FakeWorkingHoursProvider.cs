using System;
using System.Collections.Generic;
using System.Linq;
using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared;
using NSubstitute.Core;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeWorkingHoursProvider
{
    private static readonly FederatedUser FakeAdmin = FakeUserProvider.GetUserWithNameFakeoslav();
    public static IEnumerable<WorkingHours> GetCurrent(DateTimeOffset baseDate) =>
        new [] { GetCurrentForBen(baseDate), GetCurrentForPhilip(baseDate), GetCurrentForFakeoslav(baseDate)};

    public static WorkingHours GetCurrentForBen(DateTimeOffset baseDate) =>
        Create(Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2018, 3, 21),
            baseDate.AddYears(1), TimeSpan.FromHours(8));

    public static WorkingHours GetCurrentForPhilip(DateTimeOffset baseDate) =>
        Create(Guid.NewGuid(), FakeUserProvider.PhilipId, DateTimeOffsetExtensions.CreateFromDate(2018, 6, 18),
            baseDate.AddYears(2), TimeSpan.FromHours(4));

    public static WorkingHours GetCurrentForFakeoslav(DateTimeOffset baseDate) =>
        Create(Guid.NewGuid(), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2020, 5, 21),
            baseDate.AddYears(4), TimeSpan.FromHours(8));

    public static IEnumerable<WorkingHours> GetDeprecatedForBen()
    {
        var workingHoursToDeprecate = new[]
        {
            Create(Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2010, 1, 5),
                DateTimeOffsetExtensions.CreateFromDate(2012, 1, 6), TimeSpan.FromHours(8)),
            Create(Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2017, 1, 5),
                DateTimeOffsetExtensions.CreateFromDate(2018, 3, 20), TimeSpan.FromHours(8)),
        };
        return workingHoursToDeprecate;
    }
    
    public static IEnumerable<WorkingHours> GetDeprecatedForPhilip()
    {
        var workingHoursToDeprecate = new[]
        {
            Create(Guid.NewGuid(), FakeUserProvider.PhilipId, DateTimeOffsetExtensions.CreateFromDate(2015, 8, 30),
                DateTimeOffsetExtensions.CreateFromDate(2018, 6, 17), TimeSpan.FromHours(4))
        };
        return workingHoursToDeprecate;
    }

    public static IEnumerable<WorkingHours> GetDeprecatedForFakeoslav()
    {
        var workingHoursToDeprecate = new[]
        {
            Create(Guid.NewGuid(), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2015, 11, 10),
                DateTimeOffsetExtensions.CreateFromDate(2016, 1, 10), TimeSpan.FromHours(4)),
            Create(Guid.NewGuid(), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2016, 1, 11),
                DateTimeOffsetExtensions.CreateFromDate(2020, 5, 20), TimeSpan.FromHours(8)),
        };
        return workingHoursToDeprecate;
    }

    public static IEnumerable<WorkingHours> GetDeprecated() =>
        GetDeprecatedForPhilip().Union(GetDeprecatedForBen().Union(GetDeprecatedForFakeoslav()));

    public static IEnumerable<WorkingHours> GetAll(DateTimeOffset baseDate) =>
        GetCurrent(baseDate).Union(GetDeprecated());
    private static WorkingHours Create(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset? dateTo, TimeSpan duration) => 
        WorkingHours.CreateWorkingHours(WorkingHoursCreated.Create(
            workingHoursId, userId, dateFrom, dateTo, duration, FakeAdmin)
        );
}