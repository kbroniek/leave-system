namespace LeaveSystem.Domain.UnitTests.LeaveRequests.Rejecting;
using System;
using System.Net;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Rejecting;
using LeaveSystem.Shared.Dto;
using Moq;

public class RejectLeaveRequestServiceTests
{
    private readonly Mock<ReadService> mockReadService = new(null, null);
    private readonly Mock<WriteService> mockWriteService = new(null);
    private readonly RejectLeaveRequestService rejectLeaveRequestService;
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly LeaveRequestUserDto user = new("fakeUserId", "fakeUserName");

    public RejectLeaveRequestServiceTests() =>
        rejectLeaveRequestService = new RejectLeaveRequestService(mockReadService.Object, mockWriteService.Object);

    [Fact]
    public async Task Reject_ShouldReturnError_WhenLeaveRequestNotFound()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        mockReadService
            .Setup(repo => repo.FindById<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(new Error("Not Found", HttpStatusCode.NotFound));

        // Act
        var result = await rejectLeaveRequestService.Reject(leaveRequestId,
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
    public async Task Reject_ShouldReturnError_WhenLeaveRequestRejectFails()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();
        leaveRequest.Setup(lr => lr.Reject(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(new Error("Reject failed", HttpStatusCode.BadRequest));

        mockReadService
            .Setup(repo => repo.FindById<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await rejectLeaveRequestService.Reject(leaveRequestId, "Some remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Reject failed", result.Error.Message);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Reject_ShouldReturnLeaveRequest_WhenSuccessfullyRejectedAndWritten()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();

        leaveRequest.Setup(lr => lr.Reject(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(leaveRequest.Object);

        mockReadService
            .Setup(repo => repo.FindById<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        mockWriteService
            .Setup(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await rejectLeaveRequestService.Reject(leaveRequestId, "Some remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(leaveRequest.Object, result.Value);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
