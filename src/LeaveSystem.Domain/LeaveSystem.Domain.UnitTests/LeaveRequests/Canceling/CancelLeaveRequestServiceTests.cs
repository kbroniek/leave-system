namespace LeaveSystem.Domain.UnitTests.LeaveRequests.Canceling;
using System;
using System.Net;
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
    private readonly LeaveRequestUserDto user = new("fakeUserId", "fakeUserName");

    public CancelLeaveRequestServiceTests() =>
        cancelLeaveRequestService = new CancelLeaveRequestService(mockReadService.Object, mockWriteService.Object, mockTimeProvider.Object);

    [Fact]
    public async Task Cancel_ShouldReturnError_WhenLeaveRequestNotFound()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        mockReadService
            .Setup(repo => repo.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(new Error("Not Found", HttpStatusCode.NotFound));

        // Act
        var result = await cancelLeaveRequestService.Cancel(leaveRequestId,
                                                                 "Some remarks",
                                                                 user,
                                                                 DateTimeOffset.Now,
                                                                 cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Not Found", result.Error.Message);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_ShouldReturnError_WhenLeaveRequestCancelFails()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();
        leaveRequest.Setup(lr => lr.Cancel(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(new Error("Cancel failed", HttpStatusCode.BadRequest));

        mockReadService
            .Setup(repo => repo.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await cancelLeaveRequestService.Cancel(leaveRequestId, "Some remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cancel failed", result.Error.Message);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancel_ShouldReturnLeaveRequest_WhenSuccessfullyCanceledAndWritten()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();

        leaveRequest.Setup(lr => lr.Cancel(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(leaveRequest.Object);

        mockReadService
            .Setup(repo => repo.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        mockWriteService
            .Setup(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await cancelLeaveRequestService.Cancel(leaveRequestId, "Some remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(leaveRequest.Object, result.Value);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
