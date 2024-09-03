namespace LeaveSystem.Domain.UnitTests.LeaveRequests.Canceling;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Canceling;
using LeaveSystem.Shared.Dto;
using Moq;

public class CancelLeaveRequestServiceTests
{
    private readonly Mock<ReadService> mockReadService = new(null, null);
    private readonly Mock<WriteService> mockWriteService = new(null);
    private readonly Mock<TimeProvider> mockTimeProvider = new();
    private readonly CancelLeaveRequestService cancelLeaveRequestService;
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly DateTimeOffset now = DateTimeOffset.Parse("2024-09-02T13:57:31.2311892-04:00");
    private readonly LeaveRequestUserDto user = new("fakeUserId", "fakeUserName");

    public CancelLeaveRequestServiceTests()
    {
        cancelLeaveRequestService = new CancelLeaveRequestService(mockReadService.Object, mockWriteService.Object, mockTimeProvider.Object);
        mockTimeProvider.Setup(x => x.GetUtcNow())
            .Returns(now);
    }

    [Fact]
    public async Task Cancel_ShouldReturnSuccess_WhenLeaveRequestIsSuccessfullyCancelled()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();
        leaveRequest.Setup(lr => lr.DateFrom).Returns(DateOnly.FromDateTime(now.AddDays(1).Date));
        leaveRequest.Setup(lr => lr.Cancel(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(leaveRequest.Object);

        mockReadService
            .Setup(rs => rs.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);


        mockWriteService
            .Setup(ws => ws.Write(It.IsAny<LeaveRequest>(), cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await cancelLeaveRequestService.Cancel(leaveRequestId, "Remarks", user, now, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(leaveRequest.Object, result.Value);
        mockWriteService.Verify(ws => ws.Write(It.IsAny<LeaveRequest>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Cancel_ShouldReturnError_WhenLeaveRequestIsNotFound()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        mockReadService
            .Setup(rs => rs.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(new Error("Not Found", HttpStatusCode.NotFound));

        // Act
        var result = await cancelLeaveRequestService.Cancel(leaveRequestId, "Remarks", user, now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Not Found", result.Error.Message);
        mockWriteService.Verify(ws => ws.Write(It.IsAny<LeaveRequest>(), cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Cancel_ShouldReturnForbiddenError_WhenLeaveRequestIsInThePast()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();
        leaveRequest.Setup(lr => lr.DateFrom).Returns(DateOnly.FromDateTime(now.AddDays(-1).Date));  // Past date

        mockReadService
            .Setup(rs => rs.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await cancelLeaveRequestService.Cancel(leaveRequestId, "Remarks", user, now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Forbidden, result.Error.HttpStatusCode);
        Assert.Equal("Canceling of past leave requests is not allowed.", result.Error.Message);
        mockWriteService.Verify(ws => ws.Write(It.IsAny<LeaveRequest>(), cancellationToken), Times.Never);
    }

    [Fact]
    public async Task Cancel_ShouldReturnError_WhenCancelationFails()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();
        var nowDateOnly = DateOnly.FromDateTime(now.AddDays(1).Date);
        leaveRequest.Setup(lr => lr.DateFrom).Returns(nowDateOnly);
        leaveRequest.Setup(lr => lr.Cancel(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(new Error("Cancel failed", HttpStatusCode.BadRequest));

        mockReadService
            .Setup(rs => rs.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        mockTimeProvider
            .Setup(tp => tp.GetUtcNow())
            .Returns(DateTime.UtcNow);

        // Act
        var result = await cancelLeaveRequestService.Cancel(leaveRequestId, "Remarks", user, now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cancel failed", result.Error.Message);
        Assert.Equal(HttpStatusCode.BadRequest, result.Error.HttpStatusCode);
        mockWriteService.Verify(ws => ws.Write(It.IsAny<LeaveRequest>(), cancellationToken), Times.Never);
    }
}
