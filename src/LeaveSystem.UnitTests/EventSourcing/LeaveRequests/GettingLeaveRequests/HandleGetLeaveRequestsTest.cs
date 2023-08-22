using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LeaveSystem.Db;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using LeaveSystem.UnitTests.TestHelpers;
using Marten;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.GettingLeaveRequests;

public class HandleGetLeaveRequestsTest
{
    private static readonly TimeSpan WorkingHours = WorkingHoursCollection.DefaultWorkingHours;

    private async Task<HandleGetLeaveRequests> GetSut(IDocumentSession documentSession)
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        return new(documentSession, dbContext);
    }

    [Fact]
    public async Task WhenMoreThenZeroStatuses_ThenReturnSameStatuses()
    {
        //Given
        var documentSessionMock = new Mock<IDocumentSession>();
        var shortInfo1 = GetLeaveRequestShortInfo(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 4, 0, 0, 0, TimeSpan.Zero),
            WorkingHours * 3,
            FakeLeaveTypeProvider.FakeSickLeaveId,
            FakeUserProvider.GetUserWithNameFakeoslav());
        var shortInfo2 = GetLeaveRequestShortInfo(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 4, 0, 0, 0, TimeSpan.Zero),
            WorkingHours * 5,
            FakeLeaveTypeProvider.FakeOnDemandLeaveId,
            FakeUserProvider.GetUserWithNameFakeoslav(), LeaveRequestStatus.Canceled);
        var shortInfo3 = GetLeaveRequestShortInfo(
            Guid.NewGuid(),
            new DateTimeOffset(2023, 1, 3, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 5, 0, 0, 0, TimeSpan.Zero),
            WorkingHours * 6,
            FakeLeaveTypeProvider.FakeSickLeaveId,
            FakeUserProvider.GetUserWithNameFakeoslav(), LeaveRequestStatus.Rejected);
        documentSessionMock.Setup(x => x.Query<LeaveRequestShortInfo>())
            .Returns(new MartenQueryableStub<LeaveRequestShortInfo>(new List<LeaveRequestShortInfo>
            {
                shortInfo1, shortInfo2, shortInfo3
            }));
        var request = GetLeaveRequests.Create(
            null,
            null,
            new DateTimeOffset(2023, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 6, 0, 0, 0, TimeSpan.Zero),
            null,
            new[] { LeaveRequestStatus.Pending, LeaveRequestStatus.Rejected },
            null,
            null,
            FakeUserProvider.GetUserWithNameFakeoslav()
        );
        var sut = await GetSut(documentSessionMock.Object);
        //When
        var results = await sut.Handle(request, CancellationToken.None);
        results.Should().BeEquivalentTo(new []
        {
            shortInfo1, shortInfo3
        });
        documentSessionMock.Verify(x => x.Query<LeaveRequestShortInfo>());
    }

    private LeaveRequestShortInfo GetLeaveRequestShortInfo(Guid leaveRequestId, DateTimeOffset dateFrom,
        DateTimeOffset dateTo, TimeSpan duration, Guid leaveTypeId, FederatedUser createdBy,
        LeaveRequestStatus status = LeaveRequestStatus.Pending)
    {
        var leaveRequestCreated = LeaveRequestCreated.Create(
            leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, string.Empty, createdBy
        );
        var leaveRequestShortInfo = new LeaveRequestShortInfo();
        leaveRequestShortInfo.Apply(leaveRequestCreated);
        switch (status)
        {
            case LeaveRequestStatus.Accepted:
                leaveRequestShortInfo.Apply(LeaveRequestAccepted.Create(leaveRequestId, string.Empty, createdBy));
                break;
            case LeaveRequestStatus.Canceled:
                leaveRequestShortInfo.Apply(LeaveRequestCanceled.Create(leaveRequestId, string.Empty, createdBy));
                break;
            case LeaveRequestStatus.Rejected:
                leaveRequestShortInfo.Apply(LeaveRequestRejected.Create(leaveRequestId, string.Empty, createdBy));
                break;
            case LeaveRequestStatus.Pending:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        return leaveRequestShortInfo;
    }
}