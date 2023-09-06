using System;
using System.Collections.Generic;
using System.Linq;
using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;
using LeaveSystem.Shared;
using NSubstitute.Core;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeWorkingHoursProvider
{
    public static IEnumerable<WorkingHours> GetCurrent() =>
        FakeWorkingHoursCreatedProvider.GetAll().Select(WorkingHours.CreateWorkingHours);

    public static WorkingHours GetCurrentForBen() => WorkingHours.CreateWorkingHours(
        FakeWorkingHoursCreatedProvider.GetForBen()
    );

    public static WorkingHours GetCurrentForPhilip() => WorkingHours.CreateWorkingHours(
        FakeWorkingHoursCreatedProvider.GetForPhilip()
    );

    public static WorkingHours GetCurrentForFakeoslav() => WorkingHours.CreateWorkingHours(
        FakeWorkingHoursCreatedProvider.GetForFakeoslav()
    );

    public static IEnumerable<WorkingHours> GetDeprecatedForBen()
    {
        var workingHoursToDeprecate = new[]
        {
            Create(Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2010, 1, 5),
                DateTimeOffsetExtensions.CreateFromDate(2012, 1, 6), TimeSpan.FromHours(8)),
            Create(Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2017, 1, 5),
                DateTimeOffsetExtensions.CreateFromDate(2018, 3, 20), TimeSpan.FromHours(8)),
        };
        foreach (var workingHours in workingHoursToDeprecate)
        {
            workingHours.Deprecate();
        }
        return workingHoursToDeprecate;
    }
    
    public static IEnumerable<WorkingHours> GetDeprecatedForPhilip()
    {
        var workingHoursToDeprecate = new[]
        {
            Create(Guid.NewGuid(), FakeUserProvider.PhilipId, DateTimeOffsetExtensions.CreateFromDate(2018, 8, 30),
                DateTimeOffsetExtensions.CreateFromDate(2023, 6, 17), TimeSpan.FromHours(4)),
        };
        foreach (var workingHours in workingHoursToDeprecate)
        {
            workingHours.Deprecate();
        }
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
        foreach (var workingHours in workingHoursToDeprecate)
        {
            workingHours.Deprecate();
        }
        return workingHoursToDeprecate;
    }

    public static IEnumerable<WorkingHours> GetDeprecated() =>
        GetDeprecatedForPhilip().Union(GetDeprecatedForBen().Union(GetDeprecatedForFakeoslav()));

    public static IEnumerable<WorkingHours> GetAll() =>
        GetCurrent().Union(GetDeprecated());
    private static WorkingHours Create(Guid workingHoursId, string userId, DateTimeOffset dateFrom, DateTimeOffset dateTo, TimeSpan? duration) => 
        WorkingHours.CreateWorkingHours(WorkingHoursCreated.Create(
            workingHoursId, userId, dateFrom, dateTo, duration)
        );
}