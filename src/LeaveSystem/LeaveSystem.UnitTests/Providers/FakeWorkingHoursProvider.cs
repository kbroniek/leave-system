using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;
using LeaveSystem.Shared;

namespace LeaveSystem.UnitTests.Providers;

public static class FakeWorkingHoursProvider
{
    private static readonly FederatedUser FakeAdmin = FakeUserProvider.GetUserWithNameFakeoslav();
    public static IEnumerable<WorkingHours> GetCurrent(DateTimeOffset baseDate) =>
        new[] { GetCurrentForBen(baseDate), GetCurrentForPhilip(baseDate), GetCurrentForFakeoslav(baseDate) };

    public static WorkingHours GetCurrentForBen(DateTimeOffset baseDate) =>
        Create(Guid.Parse("8FBDEFBF-1F4B-4A6D-B7C1-67288C5113D7"), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2018, 3, 21),
            baseDate.AddYears(1), TimeSpan.FromHours(8));

    public static WorkingHours GetCurrentForPhilip(DateTimeOffset baseDate) =>
        Create(Guid.Parse("18D911F7-3007-4F48-8496-BAE1AAA09403"), FakeUserProvider.PhilipId, DateTimeOffsetExtensions.CreateFromDate(2018, 6, 18),
            baseDate.AddYears(2), TimeSpan.FromHours(4));

    public static WorkingHours GetCurrentForFakeoslav(DateTimeOffset baseDate) =>
        Create(Guid.Parse("4086246C-528B-41F1-9DC4-DE4BEC7361CB"), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2020, 5, 21),
            baseDate.AddYears(4), TimeSpan.FromHours(8));

    public static IEnumerable<WorkingHours> GetDeprecatedForBen()
    {
        var workingHoursToDeprecate = new[]
        {
            Create(Guid.Parse("1F75D228-3071-48C1-8A5D-60A663BD1C5F"), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2010, 1, 5),
                DateTimeOffsetExtensions.CreateFromDate(2012, 1, 6), TimeSpan.FromHours(8)),
            Create(Guid.Parse("F22ADA70-0B04-4126-89E0-6E234D8B15F2"), FakeUserProvider.BenId, DateTimeOffsetExtensions.CreateFromDate(2017, 1, 5),
                DateTimeOffsetExtensions.CreateFromDate(2018, 3, 20), TimeSpan.FromHours(8)),
        };
        return workingHoursToDeprecate;
    }

    public static IEnumerable<WorkingHours> GetDeprecatedForPhilip()
    {
        var workingHoursToDeprecate = new[]
        {
            Create(Guid.Parse("DC85ABFA-49DA-4D75-9028-3D5B978E4D34"), FakeUserProvider.PhilipId, DateTimeOffsetExtensions.CreateFromDate(2015, 8, 30),
                DateTimeOffsetExtensions.CreateFromDate(2018, 6, 17), TimeSpan.FromHours(4))
        };
        return workingHoursToDeprecate;
    }

    public static IEnumerable<WorkingHours> GetDeprecatedForFakeoslav()
    {
        var workingHoursToDeprecate = new[]
        {
            Create(Guid.Parse("36E311AD-CF44-4D46-81CD-DB20549641CC"), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2015, 11, 10),
                DateTimeOffsetExtensions.CreateFromDate(2016, 1, 10), TimeSpan.FromHours(4)),
            Create(Guid.Parse("E3AA0A11-2F63-4136-9BC6-EEB4B0230907"), FakeUserProvider.FakseoslavId, DateTimeOffsetExtensions.CreateFromDate(2016, 1, 11),
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
