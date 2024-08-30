namespace LeaveSystem.Domain.UnitTests.LeaveRequests.Accepting;
using System;
using System.Net;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Shared.Dto;
using Moq;

public class AcceptLeaveRequestServiceTests
{
    private readonly Mock<ReadService> mockReadService = new(null, null);
    private readonly Mock<WriteService> mockWriteService = new(null);
    private readonly AcceptLeaveRequestService acceptLeaveRequestService;
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly LeaveRequestUserDto user = new("fakeUserId", "fakeUserName");

    public AcceptLeaveRequestServiceTests() =>
        acceptLeaveRequestService = new AcceptLeaveRequestService(mockReadService.Object, mockWriteService.Object);

    [Fact]
    public async Task AcceptAsync_ShouldReturnError_WhenLeaveRequestNotFound()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        mockReadService
            .Setup(repo => repo.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(new Error("Not Found", HttpStatusCode.NotFound));

        // Act
        var result = await acceptLeaveRequestService.AcceptAsync(leaveRequestId,
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
    public async Task AcceptAsync_ShouldReturnError_WhenLeaveRequestAcceptFails()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();
        leaveRequest.Setup(lr => lr.Accept(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(new Error("Accept failed", HttpStatusCode.BadRequest));

        mockReadService
            .Setup(repo => repo.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await acceptLeaveRequestService.AcceptAsync(leaveRequestId, "Some remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Accept failed", result.Error.Message);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnLeaveRequest_WhenSuccessfullyAcceptedAndWritten()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();

        leaveRequest.Setup(lr => lr.Accept(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(leaveRequest.Object);

        mockReadService
            .Setup(repo => repo.FindByIdAsync<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        mockWriteService
            .Setup(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await acceptLeaveRequestService.AcceptAsync(leaveRequestId, "Some remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(leaveRequest.Object, result.Value);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
