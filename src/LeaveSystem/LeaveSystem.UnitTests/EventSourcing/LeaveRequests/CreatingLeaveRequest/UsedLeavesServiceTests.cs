using FluentAssertions;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Extensions;
using LeaveSystem.UnitTests.Providers;
using Marten;
using Marten.Events;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest.Tests;

public class UsedLeavesServiceTests
{
    private readonly Mock<IDocumentSession> documentSessionMock = new();
    private readonly Mock<IEventStore> eventStoreMock = new();
    public UsedLeavesServiceTests()
    {
        documentSessionMock
            .SetupGet(v => v.Events)
            .Returns(eventStoreMock.Object);
    }

    [Theory]
    [InlineData("2023-07-12", "2023-07-13")]
    [InlineData("2023-01-01", "2023-01-02")]
    [InlineData("2023-12-30", "2023-12-31")]
    [InlineData("2023-01-01", "2023-12-31")]
    public async Task WhenValidDate_ThenGetUsedLeavesDuration(string dateFrom, string dateTo)
    {
        // Given
        Guid leaveTypeId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139620");
        Guid leaveRequestId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139621");
        const string userId = "fakeUserId";
        LeaveRequestCreated fakeEvent = GetEvent(dateFrom, dateTo, leaveTypeId, leaveRequestId, userId);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock
            .Setup_QueryRawEventDataOnly(new[] { fakeEvent })
            .Verifiable();
        eventStoreMock
            .Setup_AggregateStreamAsync(entity, leaveRequestId)
            .Verifiable();
        var sut = GetSut();
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        // When
        var result = await sut.GetUsedLeavesDuration(now.GetFirstDayOfYear(), now.GetLastDayOfYear(), userId, leaveTypeId, Enumerable.Empty<Guid>());
        // Then
        result.Should().Be(TimeSpan.FromHours(16));
        eventStoreMock.VerifyAll();
    }

    [Theory]
    [InlineData("2022-07-12", "2022-07-13")]
    [InlineData("2024-01-01", "2024-01-02")]
    [InlineData("2022-12-30", "2022-12-31")]
    [InlineData("2022-01-01", "2022-12-31")]
    public async Task WhenInvalidDate_ThenGetZeroDuration(string dateFrom, string dateTo)
    {
        // Given
        Guid leaveTypeId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139622");
        Guid leaveRequestId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139623");
        const string userId = "fakeUserId";
        LeaveRequestCreated fakeEvent = GetEvent(dateFrom, dateTo, leaveTypeId, leaveRequestId, userId);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock
            .Setup_QueryRawEventDataOnly(new[] { fakeEvent })
            .Verifiable();
        var sut = GetSut();
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        // When
        var result = await sut.GetUsedLeavesDuration(now.GetFirstDayOfYear(), now.GetLastDayOfYear(), userId, leaveTypeId, Enumerable.Empty<Guid>());
        // Then
        result.Should().Be(TimeSpan.Zero);
        eventStoreMock.VerifyAll();
        eventStoreMock.Verify_AggregateStreamAsync(leaveRequestId, Times.Never);
    }

    [Fact]
    public async Task WhenUserIdIsDifferent_ThenGetZeroDuration()
    {
        // Given
        Guid leaveTypeId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139624");
        Guid leaveRequestId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139625");
        LeaveRequestCreated fakeEvent = GetEvent("2023-07-12", "2023-07-13", leaveTypeId, leaveRequestId, "fakeUserId");
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock
            .Setup_QueryRawEventDataOnly(new[] { fakeEvent })
            .Verifiable();
        var sut = GetSut();
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        // When
        var result = await sut.GetUsedLeavesDuration(now.GetFirstDayOfYear(), now.GetLastDayOfYear(), "fakeAnotherUserId", leaveTypeId, Enumerable.Empty<Guid>());
        // Then
        result.Should().Be(TimeSpan.Zero);
        eventStoreMock.VerifyAll();
        eventStoreMock.Verify_AggregateStreamAsync(leaveRequestId, Times.Never);
    }

    [Fact]
    public async Task WhenLeaveTypeIdIsDifferent_ThenGetZeroDuration()
    {
        // Given
        Guid leaveTypeId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139625");
        Guid leaveRequestId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139626");
        const string userId = "fakeUserId";
        LeaveRequestCreated fakeEvent = GetEvent("2023-07-12", "2023-07-13", leaveTypeId, leaveRequestId, userId);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock
            .Setup_QueryRawEventDataOnly(new[] { fakeEvent })
            .Verifiable();
        var sut = GetSut();
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        // When
        var result = await sut.GetUsedLeavesDuration(now.GetFirstDayOfYear(), now.GetLastDayOfYear(), userId, Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139627"), Enumerable.Empty<Guid>());
        // Then
        result.Should().Be(TimeSpan.Zero);
        eventStoreMock.VerifyAll();
        eventStoreMock.Verify_AggregateStreamAsync(leaveRequestId, Times.Never);
    }

    [Fact]
    public async Task WhenLeaveRequestIsNull_ThenGetZeroDuration()
    {
        // Given
        Guid leaveTypeId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139628");
        Guid leaveRequestId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139629");
        const string userId = "fakeUserId";
        LeaveRequestCreated fakeEvent = GetEvent("2023-07-12", "2023-07-13", leaveTypeId, leaveRequestId, userId);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        eventStoreMock
            .Setup_QueryRawEventDataOnly(new[] { fakeEvent })
            .Verifiable();
        var sut = GetSut();
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        // When
        var result = await sut.GetUsedLeavesDuration(now.GetFirstDayOfYear(), now.GetLastDayOfYear(), userId, leaveTypeId, Enumerable.Empty<Guid>());
        // Then
        result.Should().Be(TimeSpan.Zero);
        eventStoreMock.VerifyAll();
        eventStoreMock.Verify_AggregateStreamAsync(leaveRequestId, Times.Once);
    }

    [Fact]
    public async Task WhenLeaveRequestIsInvalid_ThenGetZeroDuration()
    {
        // Given
        Guid leaveTypeId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139628");
        Guid leaveRequestId = Guid.Parse("2ce93c56-1c3b-49b9-b063-c9d14f139629");
        const string userId = "fakeUserId";
        LeaveRequestCreated fakeEvent = GetEvent("2023-07-12", "2023-07-13", leaveTypeId, leaveRequestId, userId);
        var entity = LeaveRequest.CreatePendingLeaveRequest(fakeEvent);
        entity.Cancel("fake remarks", FederatedUser.Create(userId, "fakeUser@fake.com", "Fakeoslav"), DateTimeOffset.Parse("2023-12-15T00:00:00.0000000+00:00"));
        eventStoreMock
            .Setup_QueryRawEventDataOnly(new[] { fakeEvent })
            .Verifiable();
        eventStoreMock
            .Setup_AggregateStreamAsync(entity, leaveRequestId)
            .Verifiable();
        var sut = GetSut();
        var now = FakeDateServiceProvider.GetDateService().GetWithoutTime();
        // When
        var result = await sut.GetUsedLeavesDuration(now.GetFirstDayOfYear(), now.GetLastDayOfYear(), userId, leaveTypeId, Enumerable.Empty<Guid>());
        // Then
        result.Should().Be(TimeSpan.Zero);
        eventStoreMock.VerifyAll();
    }

    private UsedLeavesService GetSut() => new UsedLeavesService(documentSessionMock.Object);

    private static LeaveRequestCreated GetEvent(string dateFrom, string dateTo, Guid leaveTypeId, Guid leaveRequestId, string userId)
    {
        return LeaveRequestCreated.Create(
            leaveRequestId,
            DateTimeOffset.Parse(dateFrom),
            DateTimeOffset.Parse(dateTo),
            TimeSpan.FromHours(16),
            leaveTypeId,
            "fake remarks",
            FederatedUser.Create(userId, "fakeUser@fake.com", "Fakeoslav"),
            WorkingHoursUtils.DefaultWorkingHours
        );
    }
}