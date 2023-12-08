using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.Date;
using LeaveSystem.Shared.LeaveRequests;
using LeaveSystem.Shared.WorkingHours;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CancellingLeaveRequests;

public class HandleCancelLeaveRequestTest
{
    private readonly Mock<IRepository<LeaveRequest>> repositoryMock = new();
    private readonly CancelLeaveRequest command = CancelLeaveRequest.Create(Guid.NewGuid(), "testRemarks", FederatedUser.Create("1", "john@fake.com", "John"));
    private readonly Mock<IBaseDateService> dateServiceMock = new();
    
    [Fact]
    public async Task
        GivenAcceptLeaveRequestSetup_WhenAcceptLeaveRequestHandled_ThenThrowNotFoundException()
    {
        //Given
        var handleAcceptLeaveRequest = new HandleCancelLeaveRequest(repositoryMock.Object, dateServiceMock.Object);
        //When
        var act = () =>
            handleAcceptLeaveRequest.Handle(command, CancellationToken.None);
        //Then
        await act.Should().ThrowAsync<GoldenEye.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task
        GivenAcceptLeaveRequestSetup_WhenAcceptLeaveRequestHandled_ThenUpdate()
    {
        //Given
        var now = DateTimeOffset.Now;
        var createdEvent = LeaveRequestCreated.Create(
            command.LeaveRequestId,
            now + TimeSpan.FromDays(1),
            now + TimeSpan.FromDays(4),
            TimeSpan.FromHours(8),
            Guid.NewGuid(),
            "fakeRemarks",
            command.CanceledBy,
            WorkingHoursUtils.DefaultWorkingHours
        );
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(createdEvent);
        repositoryMock
            .Setup(s => s.FindById(command.LeaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        var handleAcceptLeaveRequest = new HandleCancelLeaveRequest(repositoryMock.Object, dateServiceMock.Object);
        //When
        await handleAcceptLeaveRequest.Handle(command, CancellationToken.None);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Status = LeaveRequestStatus.Canceled,
            LastModifiedBy = command.CanceledBy,
            Remarks = new[]
            {
                new LeaveRequest.RemarksModel(createdEvent.Remarks, createdEvent.CreatedBy),
                new LeaveRequest.RemarksModel(command.Remarks, command.CanceledBy)
            },
            Version = 2
        }, o => o.ExcludingMissingMembers());
        repositoryMock.Verify(x => x.Update(leaveRequest, null, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }
}