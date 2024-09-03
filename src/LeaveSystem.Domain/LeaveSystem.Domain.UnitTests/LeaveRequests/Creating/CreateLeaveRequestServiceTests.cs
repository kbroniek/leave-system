namespace LeaveSystem.Domain.UnitTests.LeaveRequests.Creating;
using System;
using System.Threading.Tasks;
using LeaveSystem.Domain.EventSourcing;
using LeaveSystem.Domain.LeaveRequests;
using LeaveSystem.Domain.LeaveRequests.Creating;
using LeaveSystem.Domain.LeaveRequests.Creating.Validators;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Dto;
using Moq;

public class CreateLeaveRequestServiceTests
{
    private readonly Mock<WriteService> mockWriteService = new(null);
    private readonly Mock<CreateLeaveRequestValidator> mockCreateLeaveRequestValidator = new(null, null, null);
    private readonly CreateLeaveRequestService createLeaveRequestService;
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly LeaveRequestUserDto user = new("fakeUserId", "fakeUserName");

    public CreateLeaveRequestServiceTests() =>
        createLeaveRequestService = new CreateLeaveRequestService(mockCreateLeaveRequestValidator.Object, mockWriteService.Object);

    [Fact(Skip = "Remove Guard.Against.OutOfRange and use Result")]
    public async Task CreateAsync_ShouldReturnError_WhenLeaveRequestPendingFails()
    {
        // Act
        var result = await createLeaveRequestService.CreateAsync(
            Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
            TimeSpan.FromHours(8) * 5, Guid.NewGuid(), "Remarks", user,
            user, TimeSpan.FromHours(40), DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Pending failed", result.Error.Message);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnLeaveRequest_WhenSuccessfullyCreatedAndWritten()
    {
        // Arrange
        var leaveRequest = new Mock<LeaveRequest>();
        leaveRequest.Setup(lr => lr.Pending(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                                              It.IsAny<TimeSpan>(), It.IsAny<Guid>(), It.IsAny<string>(),
                                              It.IsAny<LeaveRequestUserDto>(), It.IsAny<LeaveRequestUserDto>(),
                                              It.IsAny<TimeSpan>(), It.IsAny<DateTimeOffset>()))
                    .Returns(leaveRequest.Object);
        mockCreateLeaveRequestValidator.Setup(v => v.Validate(
                It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                It.IsAny<TimeSpan>(), It.IsAny<Guid>(),
                It.IsAny<TimeSpan>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<Error>());

        mockWriteService
            .Setup(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest.Object);

        // Act
        var result = await createLeaveRequestService.CreateAsync(
            Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
            TimeSpan.FromHours(8) * 5, Guid.NewGuid(), "Remarks", user,
            user, TimeSpan.FromHours(8), DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(leaveRequest.Object, result.Value);
        mockWriteService.Verify(repo => repo.Write(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
