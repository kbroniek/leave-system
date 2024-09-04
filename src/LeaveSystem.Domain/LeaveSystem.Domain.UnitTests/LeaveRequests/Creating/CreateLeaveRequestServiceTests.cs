namespace LeaveSystem.Domain.UnitTests.LeaveRequests.Creating;
using System;
using System.Net;
using System.Threading;
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
    private readonly Mock<IReadEventsRepository> mockReadEventsRepository = new();
    private readonly Mock<WriteService> mockWriteService = new(null);
    private readonly Mock<CreateLeaveRequestValidator> mockValidator = new(null, null, null);
    private readonly CreateLeaveRequestService createLeaveRequestService;
    private readonly CancellationToken cancellationToken = CancellationToken.None;
    private readonly LeaveRequestUserDto user = new("fakeUserId", "fakeUserName");

    public CreateLeaveRequestServiceTests() =>
        createLeaveRequestService = new CreateLeaveRequestService(mockReadEventsRepository.Object, mockValidator.Object, mockWriteService.Object);

    [Fact]
    public async Task CreateAsync_ShouldReturnConflictError_WhenResourceAlreadyExists()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var mockEnumerator = new Mock<IAsyncEnumerator<IEvent>>();
        mockEnumerator.Setup(e => e.MoveNextAsync()).ReturnsAsync(true);

        mockReadEventsRepository
            .Setup(r => r.ReadStreamAsync(leaveRequestId, cancellationToken))
            .Returns(new TestAsyncEnumerable<IEvent>(mockEnumerator.Object));

        // Act
        var result = await createLeaveRequestService.CreateAsync(
            leaveRequestId, DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
            TimeSpan.FromHours(40), Guid.NewGuid(), "remarks", user, user, TimeSpan.FromHours(8), DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, result.Error.HttpStatusCode);
        Assert.Equal("The resource already exists.", result.Error.Message);
        mockWriteService.Verify(w => w.Write(It.IsAny<LeaveRequest>(), cancellationToken), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnValidationError_WhenValidationFails()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var dateFrom = DateOnly.FromDateTime(DateTime.Now);
        var dateTo = dateFrom.AddDays(5);
        var duration = TimeSpan.FromHours(40);
        var leaveTypeId = Guid.NewGuid();

        mockReadEventsRepository
            .Setup(r => r.ReadStreamAsync(leaveRequestId, cancellationToken))
            .Returns(AsyncEnumerable.Empty<IEvent>());

        mockValidator
            .Setup(v => v.Validate(leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, It.IsAny<TimeSpan>(), user.Id, cancellationToken))
            .ReturnsAsync(new Error("Validation failed", HttpStatusCode.BadRequest));

        // Act
        var result = await createLeaveRequestService.CreateAsync(
            leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, "remarks", user, user, TimeSpan.FromHours(8), DateTimeOffset.Now, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation failed", result.Error.Message);
        mockWriteService.Verify(w => w.Write(It.IsAny<LeaveRequest>(), cancellationToken), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenLeaveRequestIsSuccessfullyCreated()
    {
        // Arrange
        var leaveRequestId = Guid.NewGuid();
        var dateFrom = DateOnly.FromDateTime(DateTime.Now);
        var dateTo = dateFrom.AddDays(5);
        var duration = TimeSpan.FromHours(40);
        var leaveTypeId = Guid.NewGuid();
        var workingHours = TimeSpan.FromHours(8);
        var createdDate = DateTimeOffset.Now;
        mockReadEventsRepository
            .Setup(r => r.ReadStreamAsync(leaveRequestId, cancellationToken))
            .Returns(AsyncEnumerable.Empty<IEvent>());

        mockValidator
            .Setup(v => v.Validate(leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, workingHours, user.Id, cancellationToken))
            .ReturnsAsync(Result.Default);

        mockWriteService
            .Setup(w => w.Write(It.IsAny<LeaveRequest>(), cancellationToken))
            .ReturnsAsync(new LeaveRequest());

        // Act
        var result = await createLeaveRequestService.CreateAsync(
            leaveRequestId, dateFrom, dateTo, duration, leaveTypeId, "remarks", user, user, workingHours, createdDate, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        mockWriteService.Verify(w => w.Write(It.IsAny<LeaveRequest>(), cancellationToken), Times.Once);
    }
    // Helper class for testing async enumerables
    public class TestAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerator<T> _enumerator;

        public TestAsyncEnumerable(IAsyncEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return _enumerator;
        }
    }
}
