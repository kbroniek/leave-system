namespace LeaveSystem.Domain.UnitTests.LeaveRequests.Accepting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Accepting;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Moq;

public class AcceptLeaveRequestServiceTests
{
    private readonly Mock<ReadService> mockReadService = new(null, null);
    private readonly Mock<WriteService> mockWriteService = new(null);
    private readonly AcceptLeaveRequestService acceptLeaveRequestService;
    private readonly Mock<CreateLeaveRequestValidator> mockValidator = new(null, null, null);
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly LeaveRequestUserDto user = new("fakeUserId", "fakeUserName");

    public AcceptLeaveRequestServiceTests() =>
        acceptLeaveRequestService = new AcceptLeaveRequestService(mockReadService.Object, mockWriteService.Object, mockValidator.Object);

    [Fact]
    public async Task AcceptAsync_ShouldReturnError_WhenLeaveRequestNotFound()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        mockReadService
            .Setup(repo => repo.FindById<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(new Error("Not Found", HttpStatusCode.NotFound, ErrorCodes.RESOURCE_NOT_FOUND));

        // Act
        var result = await acceptLeaveRequestService.Accept(leaveRequestId,
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
                    .Returns(new Error("Accept failed", HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT))
                    .Verifiable(Times.Once);
        leaveRequest.Setup(lr => lr.AssignedTo).Returns(user);
        mockValidator
            .Setup(v => v.Validate(
                leaveRequestId, leaveRequest.Object.DateFrom, leaveRequest.Object.DateTo,
                leaveRequest.Object.Duration, leaveRequest.Object.LeaveTypeId,
                leaveRequest.Object.WorkingHours, It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(Result.Default);

        mockReadService
            .Setup(repo => repo.FindById<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await acceptLeaveRequestService.Accept(leaveRequestId, "Some remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        leaveRequest.VerifyAll();
        Assert.False(result.IsSuccess);
        Assert.Equal("Accept failed", result.Error.Message);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Accept_ShouldReturnValidationError_WhenValidationFails()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();

        mockReadService
            .Setup(rs => rs.FindById<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);
        leaveRequest.Setup(lr => lr.AssignedTo).Returns(user);

        mockValidator
            .Setup(v => v.Validate(
                leaveRequestId, leaveRequest.Object.DateFrom, leaveRequest.Object.DateTo,
                leaveRequest.Object.Duration, leaveRequest.Object.LeaveTypeId,
                leaveRequest.Object.WorkingHours, leaveRequest.Object.AssignedTo.Id, cancellationToken))
            .ReturnsAsync(new Error("Validation failed", HttpStatusCode.BadRequest, ErrorCodes.INVALID_INPUT));

        // Act
        var result = await acceptLeaveRequestService.Accept(leaveRequestId, "Remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation failed", result.Error.Message);
        mockWriteService.Verify(ws => ws.Write(It.IsAny<LeaveRequest>(), cancellationToken), Times.Never);
    }

    [Fact]
    public async Task AcceptAsync_ShouldReturnLeaveRequest_WhenSuccessfullyAccepted()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var leaveRequest = new Mock<LeaveRequest>();

        leaveRequest.Setup(lr => lr.Accept(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<LeaveRequestUserDto>(), It.IsAny<DateTimeOffset>()))
                    .Returns(leaveRequest.Object);
        leaveRequest.Setup(lr => lr.AssignedTo).Returns(user);

        mockValidator
            .Setup(v => v.Validate(
                leaveRequestId, leaveRequest.Object.DateFrom, leaveRequest.Object.DateTo,
                leaveRequest.Object.Duration, leaveRequest.Object.LeaveTypeId,
                leaveRequest.Object.WorkingHours, It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(Result.Default);
        mockReadService
            .Setup(repo => repo.FindById<LeaveRequest>(leaveRequestId, cancellationToken))
            .ReturnsAsync(leaveRequest.Object);

        mockWriteService
            .Setup(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await acceptLeaveRequestService.Accept(leaveRequestId, "Some remarks", user, DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(leaveRequest.Object, result.Value);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
