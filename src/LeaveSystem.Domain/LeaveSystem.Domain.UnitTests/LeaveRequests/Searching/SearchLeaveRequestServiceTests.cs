namespace LeaveSystem.Domain.UnitTests.LeaveRequests.Searching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Searching;
using LeaveSystem.Shared.Dto;
using LeaveSystem.Shared.LeaveRequests;
using Moq;
using Xunit;

public class SearchLeaveRequestServiceTests
{
    private readonly Mock<ISearchLeaveRequestRepository> mockSearchLeaveRequestRepository = new();
    private readonly Mock<ReadService> mockReadService = new(null, null);
    private readonly Mock<TimeProvider> mockTimeProvider = new();
    private readonly SearchLeaveRequestService searchLeaveRequestService;
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly LeaveRequestUserDto user = new("fakeUserId", "fakeUserName");
    private readonly DateTimeOffset now = DateTimeOffset.Parse("2024-02-01 00:00:00 +00:00");

    public SearchLeaveRequestServiceTests() =>
        searchLeaveRequestService = new SearchLeaveRequestService(mockSearchLeaveRequestRepository.Object, mockReadService.Object, mockTimeProvider.Object);

    [Fact]
    public async Task Search_ShouldReturnResults_WhenEventsAreFound()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveTypeId = Guid.NewGuid();
        var assignedToUserId = "User1";

        var pendingEvents = new List<ISearchLeaveRequestRepository.PendingEventEntity>
        {
            new(leaveRequestId, leaveTypeId, new ISearchLeaveRequestRepository.EventUserEntity(assignedToUserId), DateOnly.FromDateTime(now.Date), DateOnly.FromDateTime(now.Date.AddDays(1)), TimeSpan.FromHours(8))
        };
        var leaveRequests = new List<LeaveRequest>
        {
            new LeaveRequest().Pending(
                leaveRequestId, DateOnly.FromDateTime(now.Date), DateOnly.FromDateTime(now.Date.AddDays(1)),
                TimeSpan.FromHours(8), leaveTypeId, "fake remarks", user, user, TimeSpan.FromHours(8), now).Value
        };

        mockSearchLeaveRequestRepository
            .Setup(r => r.GetPendingEvents(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<Guid[]>(), It.IsAny<string[]>(), cancellationToken))
            .ReturnsAsync((pendingEvents, null))
            .Verifiable(Times.Once);

        mockReadService
            .Setup(r => r.FindByIds<LeaveRequest>(It.IsAny<Guid[]>(), cancellationToken))
            .ReturnsAsync(leaveRequests)
            .Verifiable(Times.Once);

        mockTimeProvider
            .Setup(t => t.GetUtcNow())
            .Returns(now);

        // Act
        var result = await searchLeaveRequestService.Search(null, null, null, null, null, null, cancellationToken);


        // Assert
        Assert.True(result.IsSuccess, "Search return error");
        var (results, _) = result.Value;
        Assert.NotEmpty(results);
        Assert.Equal(leaveRequestId, results.First().Id);
        mockSearchLeaveRequestRepository.VerifyAll();
        mockReadService.VerifyAll();
    }

    [Fact]
    public async Task Search_ShouldReturnEmptyResults_WhenNoEventsAreFound()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        mockSearchLeaveRequestRepository
            .Setup(r => r.GetPendingEvents(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<Guid[]>(), It.IsAny<string[]>(), cancellationToken))
            .ReturnsAsync((Enumerable.Empty<ISearchLeaveRequestRepository.PendingEventEntity>(), null));

        mockReadService
            .Setup(r => r.FindByIds<LeaveRequest>(It.IsAny<Guid[]>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<LeaveRequest>());

        mockTimeProvider
            .Setup(t => t.GetUtcNow())
            .Returns(now);

        // Act
        var result = await searchLeaveRequestService.Search(null, null, null, null, null, null, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess, "Search return error");
        Assert.Empty(result.Value.results);
    }

    [Fact]
    public async Task Search_ShouldUseDefaultStatuses_WhenStatusesNotProvided()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        mockSearchLeaveRequestRepository
            .Setup(r => r.GetPendingEvents(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<Guid[]>(), It.IsAny<string[]>(), cancellationToken))
            .ReturnsAsync((Enumerable.Empty<ISearchLeaveRequestRepository.PendingEventEntity>(), null));

        mockTimeProvider
            .Setup(t => t.GetUtcNow())
            .Returns(now);

        // Act
        var result = await searchLeaveRequestService.Search(null, null, null, null, null, null, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess, "Search return error");
        Assert.Contains(LeaveRequestStatus.Pending, result.Value.search.Statuses);
        Assert.Contains(LeaveRequestStatus.Accepted, result.Value.search.Statuses);
    }

    [Fact]
    public async Task Search_ShouldUseDefaultDateRange_WhenDatesNotProvided()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        mockSearchLeaveRequestRepository
            .Setup(r => r.GetPendingEvents(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<Guid[]>(), It.IsAny<string[]>(), cancellationToken))
            .ReturnsAsync((Enumerable.Empty<ISearchLeaveRequestRepository.PendingEventEntity>(), null));

        mockTimeProvider
            .Setup(t => t.GetUtcNow())
            .Returns(now);

        // Act
        var result = await searchLeaveRequestService.Search(null, null, null, null, null, null, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess, "Search return error");
        Assert.Equal(DateOnly.FromDateTime(now.AddDays(-14).Date), result.Value.search.DateFrom);
        Assert.Equal(DateOnly.FromDateTime(now.AddDays(14).Date), result.Value.search.DateTo);
    }
}
