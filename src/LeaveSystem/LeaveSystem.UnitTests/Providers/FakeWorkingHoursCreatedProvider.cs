using System;
using System.Collections;
using System.Collections.Generic;
using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeWorkingHoursCreatedProvider
{
    private static readonly FederatedUser fakeAdmin = FakeUserProvider.GetUserWithNameFakeoslav();
    public static WorkingHoursCreated GetForBen() => WorkingHoursCreated.Create(
        Guid.NewGuid(), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2020, 12, 6),
        DateTimeOffsetExtensions.CreateFromDate(2025, 12, 6), TimeSpan.FromHours(8),
        fakeAdmin
        );

    public static WorkingHoursCreated GetForPhilip() => WorkingHoursCreated.Create(
        Guid.NewGuid(), FakeUserProvider.PhilipId, DateTimeOffsetExtensions.CreateFromDate(2021, 12, 6),
        DateTimeOffsetExtensions.CreateFromDate(2023, 5, 1), TimeSpan.FromHours(8),
        fakeAdmin
        );
    
    public static WorkingHoursCreated GetForFakeoslav() => WorkingHoursCreated.Create(
        Guid.NewGuid(), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2018, 6, 6),
        DateTimeOffsetExtensions.CreateFromDate(2026, 5, 1), TimeSpan.FromHours(8),
        fakeAdmin
    );

    public static IEnumerable<WorkingHoursCreated> GetAll(bool withBen = true)
    {
        yield return GetForPhilip();
        yield return GetForFakeoslav();
        if (withBen)
        {
            yield return GetForBen();
        }
    } 
}