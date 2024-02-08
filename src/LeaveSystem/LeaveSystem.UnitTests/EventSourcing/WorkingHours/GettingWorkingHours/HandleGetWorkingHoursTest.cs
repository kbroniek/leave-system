using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using NSubstitute;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.GettingWorkingHours;

using LeaveSystem.EventSourcing.WorkingHours;
using LeaveSystem.EventSourcing.WorkingHours.CreatingWorkingHours;

public class HandleGetWorkingHoursTest
{
    private readonly IDocumentSession querySession = Substitute.For<IDocumentSession>();
    private static readonly FederatedUser FakeAdmin = FakeUserProvider.GetUserWithNameFakeoslav();
    private HandleGetWorkingHours GetSut() => new(querySession);

    private static readonly WorkingHours[] Data =
    {
        CreateWorkingHours( // 1
            Guid.Parse("462D5A93-35B1-43E2-ABB5-CC59C664ADAB"),
            FakeUserProvider.BenId,
            DateTimeOffset.Parse("2018-03-21 +00"),
            DateTimeOffset.Parse("2025-01-17 +00"),
            TimeSpan.FromHours(8)
        ),
        CreateWorkingHours( // 2
            Guid.Parse("3A6323E7-FE20-403A-A8BB-F6EEDA2D705E"),
            FakeUserProvider.PhilipId,
            DateTimeOffset.Parse("2018-06-18 +00"),
            DateTimeOffset.Parse("2026-01-17 +00"),
            TimeSpan.FromHours(4)
        ),
        CreateWorkingHours( // 3
            Guid.Parse("E4970D57-196D-441F-AB3D-702DD7DA25AB"),
            FakeUserProvider.FakseoslavId,
            DateTimeOffset.Parse("2020-05-21"),
            null,
            TimeSpan.FromHours(8)
        ),
        CreateWorkingHours( // 4
            Guid.Parse("C64D3958-08F6-4B18-B607-0482EA2562C7"),
            FakeUserProvider.BenId,
            DateTimeOffset.Parse("2010-01-05"),
            DateTimeOffset.Parse("2012-01-06"),
            TimeSpan.FromHours(8)
        ),
        CreateWorkingHours( // 5
            Guid.Parse("2F113B1D-5853-4AD4-AB2E-27739D8596E2"),
            FakeUserProvider.BenId,
            DateTimeOffset.Parse("2017-01-05"),
            DateTimeOffset.Parse("2018-03-20"),
            TimeSpan.FromHours(8)
        ),
        CreateWorkingHours( // 6
            Guid.Parse("133C6AB1-E310-4FDB-A28A-9D4F53A31FB9"),
            FakeUserProvider.PhilipId,
            DateTimeOffset.Parse("2015-08-03"),
            DateTimeOffset.Parse("2018-06-17"),
            TimeSpan.FromHours(4)
        ),
        CreateWorkingHours( // 7
            Guid.Parse("22CADF3E-493F-45E4-A526-57721E13BA0B"),
            FakeUserProvider.FakseoslavId,
            DateTimeOffset.Parse("2015-11-10"),
            DateTimeOffset.Parse("2016-01-10"),
            TimeSpan.FromHours(4)
        ),
        CreateWorkingHours( // 8
            Guid.Parse("312333E8-2A09-41DC-B821-3ACECD54BE37"),
            FakeUserProvider.FakseoslavId,
            DateTimeOffset.Parse("2016-01-11"),
            DateTimeOffset.Parse("2020-05-20"),
            TimeSpan.FromHours(8)
        )
    };

    private static WorkingHours CreateWorkingHours(Guid workingHoursId, string userId, DateTimeOffset dateFrom,
        DateTimeOffset? dateTo, TimeSpan duration) =>
        WorkingHours.CreateWorkingHours(WorkingHoursCreated.Create(
            workingHoursId, userId, dateFrom, dateTo, duration, FakeAdmin)
        );

    [Theory]
    [MemberData(nameof(Get_WhenRequestHandled_ThenReturnExpectedWorkingHours_TestData))]
    public async Task WhenRequestHandled_ThenReturnExpectedWorkingHours(
        GetWorkingHours request,
        object[] expectedData)
    {
        //Given
        this.querySession.Query<WorkingHours>().Returns(
            new MartenQueryableStub<WorkingHours>(
                Data
            ));
        var sut = this.GetSut();
        //When
        var result = await sut.Handle(request, CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(expectedData);
    }

    public static TheoryData<GetWorkingHours, object[]>
        Get_WhenRequestHandled_ThenReturnExpectedWorkingHours_TestData() =>
        new()
        {
            {
                GetWorkingHours.Create(
                    5,
                    1,
                    DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                    DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                    null,
                    FakeAdmin,
                    new[] { WorkingHoursStatus.Deprecated, WorkingHoursStatus.Current, WorkingHoursStatus.Future }
                ),
                new[]
                {
                    CreateCompareObject( // 1
                        Guid.Parse("462D5A93-35B1-43E2-ABB5-CC59C664ADAB"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2018-03-21 +00"),
                        DateTimeOffset.Parse("2025-01-17 +00"),
                        TimeSpan.FromHours(8)
                    ),
                    CreateCompareObject( // 2
                        Guid.Parse("3A6323E7-FE20-403A-A8BB-F6EEDA2D705E"),
                        FakeUserProvider.PhilipId,
                        DateTimeOffset.Parse("2018-06-18 +00"),
                        DateTimeOffset.Parse("2026-01-17 +00"),
                        TimeSpan.FromHours(4)
                    ),
                    CreateCompareObject( // 3
                        Guid.Parse("E4970D57-196D-441F-AB3D-702DD7DA25AB"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2020-05-21"),
                        null,
                        TimeSpan.FromHours(8)
                    ),
                    CreateCompareObject( // 4
                        Guid.Parse("C64D3958-08F6-4B18-B607-0482EA2562C7"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2010-01-05"),
                        DateTimeOffset.Parse("2012-01-06"),
                        TimeSpan.FromHours(8)
                    ),
                    CreateCompareObject( // 5
                        Guid.Parse("2F113B1D-5853-4AD4-AB2E-27739D8596E2"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2017-01-05"),
                        DateTimeOffset.Parse("2018-03-20"),
                        TimeSpan.FromHours(8)
                    )
                }
            },
            {
                GetWorkingHours.Create(
                    3,
                    3,
                    DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                    DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                    null,
                    FakeAdmin,
                    new[] { WorkingHoursStatus.Deprecated, WorkingHoursStatus.Current }
                ),
                new[]
                {
                    CreateCompareObject( // 7
                        Guid.Parse("22CADF3E-493F-45E4-A526-57721E13BA0B"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2015-11-10"),
                        DateTimeOffset.Parse("2016-01-10"),
                        TimeSpan.FromHours(4)
                    ),
                    CreateCompareObject( // 8
                        Guid.Parse("312333E8-2A09-41DC-B821-3ACECD54BE37"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2016-01-11"),
                        DateTimeOffset.Parse("2020-05-20"),
                        TimeSpan.FromHours(8)
                    )
                }
            },
            {
                GetWorkingHours.Create(
                    8,
                    1,
                    DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                    DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                    null,
                    FakeAdmin,
                    new[] { WorkingHoursStatus.Deprecated }
                ),
                new[]
                {
                    CreateCompareObject( // 4
                        Guid.Parse("C64D3958-08F6-4B18-B607-0482EA2562C7"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2010-01-05"),
                        DateTimeOffset.Parse("2012-01-06"),
                        TimeSpan.FromHours(8)
                    ),
                    CreateCompareObject( // 5
                        Guid.Parse("2F113B1D-5853-4AD4-AB2E-27739D8596E2"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2017-01-05"),
                        DateTimeOffset.Parse("2018-03-20"),
                        TimeSpan.FromHours(8)
                    ),
                    CreateCompareObject( // 6
                        Guid.Parse("133C6AB1-E310-4FDB-A28A-9D4F53A31FB9"),
                        FakeUserProvider.PhilipId,
                        DateTimeOffset.Parse("2015-08-03"),
                        DateTimeOffset.Parse("2018-06-17"),
                        TimeSpan.FromHours(4)
                    ),
                    CreateCompareObject( // 7
                        Guid.Parse("22CADF3E-493F-45E4-A526-57721E13BA0B"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2015-11-10"),
                        DateTimeOffset.Parse("2016-01-10"),
                        TimeSpan.FromHours(4)
                    ),
                    CreateCompareObject( // 8
                        Guid.Parse("312333E8-2A09-41DC-B821-3ACECD54BE37"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2016-01-11"),
                        DateTimeOffset.Parse("2020-05-20"),
                        TimeSpan.FromHours(8)
                    )
                }
            },
            {
                GetWorkingHours.Create(
                    8,
                    1,
                    DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                    DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                    null,
                    FakeAdmin,
                    new[] { WorkingHoursStatus.Deprecated }
                ),
                new[]
                {
                    CreateCompareObject( // 4
                        Guid.Parse("C64D3958-08F6-4B18-B607-0482EA2562C7"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2010-01-05"),
                        DateTimeOffset.Parse("2012-01-06"),
                        TimeSpan.FromHours(8)
                    ),
                    CreateCompareObject( // 5
                        Guid.Parse("2F113B1D-5853-4AD4-AB2E-27739D8596E2"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2017-01-05"),
                        DateTimeOffset.Parse("2018-03-20"),
                        TimeSpan.FromHours(8)
                    ),
                    CreateCompareObject( // 6
                        Guid.Parse("133C6AB1-E310-4FDB-A28A-9D4F53A31FB9"),
                        FakeUserProvider.PhilipId,
                        DateTimeOffset.Parse("2015-08-03"),
                        DateTimeOffset.Parse("2018-06-17"),
                        TimeSpan.FromHours(4)
                    ),
                    CreateCompareObject( // 7
                        Guid.Parse("22CADF3E-493F-45E4-A526-57721E13BA0B"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2015-11-10"),
                        DateTimeOffset.Parse("2016-01-10"),
                        TimeSpan.FromHours(4)
                    ),
                    CreateCompareObject( // 8
                        Guid.Parse("312333E8-2A09-41DC-B821-3ACECD54BE37"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2016-01-11"),
                        DateTimeOffset.Parse("2020-05-20"),
                        TimeSpan.FromHours(8)
                    )
                }
            },
            {
                GetWorkingHours.Create(
                    8,
                    1,
                    DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                    DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                    new[] { "1", "2" },
                    FakeAdmin,
                    new[] { WorkingHoursStatus.Deprecated }
                ),
                new[]
                {
                    CreateWorkingHours( // 4
                        Guid.Parse("C64D3958-08F6-4B18-B607-0482EA2562C7"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2010-01-05"),
                        DateTimeOffset.Parse("2012-01-06"),
                        TimeSpan.FromHours(8)
                    ),
                    CreateWorkingHours( // 5
                        Guid.Parse("2F113B1D-5853-4AD4-AB2E-27739D8596E2"),
                        FakeUserProvider.BenId,
                        DateTimeOffset.Parse("2017-01-05"),
                        DateTimeOffset.Parse("2018-03-20"),
                        TimeSpan.FromHours(8)
                    ),
                    CreateCompareObject( // 7
                        Guid.Parse("22CADF3E-493F-45E4-A526-57721E13BA0B"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2015-11-10"),
                        DateTimeOffset.Parse("2016-01-10"),
                        TimeSpan.FromHours(4)
                    ),
                    CreateCompareObject( // 8
                        Guid.Parse("312333E8-2A09-41DC-B821-3ACECD54BE37"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2016-01-11"),
                        DateTimeOffset.Parse("2020-05-20"),
                        TimeSpan.FromHours(8)
                    )
                }
            },
            {
                GetWorkingHours.Create(
                    8,
                    1,
                    DateTimeOffset.Parse("2025-02-01"),
                    DateTimeOffsetExtensions.CreateFromDate(2027, 1, 1),
                    null,
                    FakeAdmin,
                    null
                ),
                new[]
                {
                    CreateCompareObject( // 2
                        Guid.Parse("3A6323E7-FE20-403A-A8BB-F6EEDA2D705E"),
                        FakeUserProvider.PhilipId,
                        DateTimeOffset.Parse("2018-06-18 +00"),
                        DateTimeOffset.Parse("2026-01-17 +00"),
                        TimeSpan.FromHours(4)
                    ),
                    CreateCompareObject( // 3
                        Guid.Parse("E4970D57-196D-441F-AB3D-702DD7DA25AB"),
                        FakeUserProvider.FakseoslavId,
                        DateTimeOffset.Parse("2020-05-21"),
                        null,
                        TimeSpan.FromHours(8)
                    )
                }
            }
        };

    private static object CreateCompareObject(Guid workingHoursId, string userId, DateTimeOffset dateFrom,
        DateTimeOffset? dateTo, TimeSpan duration) =>
        new
        {
            Id = workingHoursId,
            UserId = userId,
            Duration = duration,
            DateFrom = dateFrom,
            DateTo = dateTo
        };
}
