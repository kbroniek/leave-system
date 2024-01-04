using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.ImTools;
using FluentAssertions;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using NSubstitute;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.GettingWorkingHours;

public class HandleGetWorkingHoursTest
{
    private IDocumentSession querySession;

    private HandleGetWorkingHours GetSut() => new(querySession);

    private static readonly LeaveSystem.EventSourcing.WorkingHours.WorkingHours[] Data = FakeWorkingHoursProvider
        .GetAll(FakeDateServiceProvider.GetDateService().UtcNowWithoutTime())
        .ToArray();


    [Theory]
    [MemberData(nameof(Get_WhenRequestHandled_ThenReturnExpectedWorkingHours_TestData))]
    public async Task WhenRequestHandled_ThenReturnExpectedWorkingHours(
        GetWorkingHours request,
        IEnumerable<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> expectedData)
    {
        //Given
        querySession = Substitute.For<IDocumentSession>();
        querySession.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>().Returns(
            new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
                Data
            ));
        var sut = GetSut();
        //When
        var result = await sut.Handle(request, CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(expectedData);
    }

    public static IEnumerable<object[]> Get_WhenRequestHandled_ThenReturnExpectedWorkingHours_TestData()
    {
        var user = FakeUserProvider.GetUserWithNameFakeoslav();
        yield return new object[]
        {
            GetWorkingHours.Create(
                5,
                1,
                DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                null,
                user,
                new[] { WorkingHoursStatus.Deprecated, WorkingHoursStatus.Current, WorkingHoursStatus.Future }
            ),
            new[]
            {
                Data[0],
                Data[1],
                Data[2],
                Data[3],
                Data[4]
            }
        };
        yield return new object[]
        {
            GetWorkingHours.Create(
                3,
                3,
                DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                null,
                user,
                new[] { WorkingHoursStatus.Deprecated, WorkingHoursStatus.Current }
            ),
            new[]
            {
                Data[6],
                Data[7]
            }
        };
        yield return new object[]
        {
            GetWorkingHours.Create(
                8,
                1,
                DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                null,
                user,
                new[] { WorkingHoursStatus.Deprecated }
            ),
            new[]
            {
                Data[3],
                Data[4],
                Data[5],
                Data[6],
                Data[7]
            }
        };
        yield return new object[]
        {
            GetWorkingHours.Create(
                8,
                1,
                DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                null,
                user,
                new[] { WorkingHoursStatus.Deprecated }
            ),
            new[]
            {
                Data[3],
                Data[4],
                Data[5],
                Data[6],
                Data[7]
            }
        };
        yield return new object[]
        {
            GetWorkingHours.Create(
                8,
                1,
                DateTimeOffsetExtensions.CreateFromDate(2000, 1, 1),
                DateTimeOffsetExtensions.CreateFromDate(2030, 1, 1),
                new[] { "1", "2" },
                user,
                new[] { WorkingHoursStatus.Deprecated }
            ),
            new[]
            {
                Data[4],
                Data[5],
                Data[6],
                Data[7]
            }
        };
        yield return new object[]
        {
            GetWorkingHours.Create(
                8,
                1,
                DateTimeOffsetExtensions.CreateFromDate(2025, 1, 1),
                DateTimeOffsetExtensions.CreateFromDate(2027, 1, 1),
                null,
                user,
                null
            ),
            new[]
            {
                Data[1],
                Data[2]
            }
        };
    }
}