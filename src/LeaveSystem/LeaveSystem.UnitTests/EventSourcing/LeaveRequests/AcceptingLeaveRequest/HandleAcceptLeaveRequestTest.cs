using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using Moq;
using LeaveRequest = LeaveSystem.EventSourcing.LeaveRequests.LeaveRequest;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.AcceptingLeaveRequest;

public class HandleAcceptLeaveRequestTest
{
    private readonly Mock<IRepository<LeaveRequest>> repositoryMock = new();
    private readonly AcceptLeaveRequest command = AcceptLeaveRequest.Create(Guid.NewGuid(), "testRemarks", FederatedUser.Create("1", "john@fake.com", "John"));

    [Fact]
    public async Task
        GivenAcceptLeaveRequestSetup_WhenAcceptLeaveRequestHandled_ThenThrowNotFoundException()
    {
        //Given
        var handleAcceptLeaveRequest = new HandleAcceptLeaveRequest(repositoryMock.Object);
        //When
        var act = () =>
            handleAcceptLeaveRequest.Handle(command, CancellationToken.None);
        //Then
        await act.Should().ThrowAsync<GoldenEye.Backend.Core.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task
        GivenAcceptLeaveRequestSetup_WhenAcceptLeaveRequestHandled_ThenUpdate()
    {
        //Given
        var now = DateTimeOffset.Now;
        var createdEvent = LeaveRequestCreated.Create(
            command.LeaveRequestId,
            now + TimeSpan.FromDays(4),
            now + TimeSpan.FromDays(6),
            TimeSpan.FromHours(8),
            Guid.NewGuid(),
            "fakeRemarks",
            command.AcceptedBy,
            WorkingHoursUtils.DefaultWorkingHours
            );
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(createdEvent);
        repositoryMock
            .Setup(s => s.FindByIdAsync(command.LeaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        var handleAcceptLeaveRequest = new HandleAcceptLeaveRequest(repositoryMock.Object);
        //When
        await handleAcceptLeaveRequest.Handle(command, CancellationToken.None);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Status = LeaveRequestStatus.Accepted,
            LastModifiedBy = command.AcceptedBy,
            Remarks = new[]
            {
                new LeaveRequest.RemarksModel(createdEvent.Remarks!, createdEvent.CreatedBy),
                new LeaveRequest.RemarksModel(command.Remarks!, command.AcceptedBy)
            },
            Version = 2
        }, o => o.ExcludingMissingMembers());
        repositoryMock.Verify(x => x.UpdateAsync(leaveRequest, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
