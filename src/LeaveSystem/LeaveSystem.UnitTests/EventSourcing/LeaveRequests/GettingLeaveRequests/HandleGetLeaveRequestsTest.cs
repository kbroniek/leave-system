using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.GettingLeaveRequests;

public class HandleGetLeaveRequestsTest
{
    private static readonly TimeSpan WorkingHours = TimeSpan.FromHours(8);
    private static readonly DateTimeOffset Now = DateTimeOffset.Now;
    private static readonly LeaveRequestShortInfo[] Data = FakeLeaveRequestShortInfoProvider.Get(Now);

    private static async Task<HandleGetLeaveRequests> GetSut(IDocumentSession documentSession)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        return new(documentSession, dbContext);
    }

    [Theory]
    [MemberData(nameof(Get_WhenMoreThenZeroStatuses_ThenReturnSameStatuses_TestData))]
    public async Task WhenMoreThenZeroStatuses_ThenReturnSameStatuses(
        GetLeaveRequests request, LeaveRequestShortInfo[] expectedInfo)
    {
        //Given
        var documentSessionMock = new Mock<IDocumentSession>();
        documentSessionMock.Setup(x => x.Query<LeaveRequestShortInfo>())
            .Returns(new MartenQueryableStub<LeaveRequestShortInfo>(Data));
        var sut = await GetSut(documentSessionMock.Object);
        //When
        var results = await sut.Handle(request, CancellationToken.None);
        results.Should().BeEquivalentTo(expectedInfo);
        documentSessionMock.Verify(x => x.Query<LeaveRequestShortInfo>());
    }

    public static IEnumerable<object[]> Get_WhenMoreThenZeroStatuses_ThenReturnSameStatuses_TestData()
    {
        var user = FakeUserProvider.GetUserWithNameFakeoslav();
        yield return new object[]
        {
            GetLeaveRequests.Create(
                1,
                5,
                Now.AddYears(-5),
                Now.AddYears(5),
                null,
                new [] { LeaveRequestStatus.Accepted , LeaveRequestStatus.Canceled, LeaveRequestStatus.Deprecated, LeaveRequestStatus.Pending, LeaveRequestStatus.Rejected},
                null,
                null,
                user
            ),
            new LeaveRequestShortInfo[]
            {
                Data[0],
                Data[1],
                Data[2],
                Data[3],
                Data[4]
            }
        };
    }
}