using FluentAssertions;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeaveSystem.EventSourcing.LeaveRequests.CancelingLeaveRequest;
using Xunit;
using LeaveRequest = LeaveSystem.EventSourcing.LeaveRequests.LeaveRequest;

namespace LeaveSystem.UnitTests;

public class HandleAcceptLeaveRequestTest
{
    private readonly Mock<IRepository<LeaveRequest>> repositoryMock = new();
    private readonly AcceptLeaveRequest command = AcceptLeaveRequest.Create(Guid.NewGuid(), "testRemarks", FederatedUser.Create("1", "john@fake.com", "John"));
    
    [Fact]
    public async Task
        GivenAcceptLeaveRequestSetup_WhenAcceptLeaveRequestHandled_ThenThrowNotFoundException()
    {
        //Given
        var handleAcceptLeaveRequest = new HandleBasicAcceptLeaveRequest(repositoryMock.Object);
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
        var createdEvent = LeaveRequestCreated.Create(
            command.LeaveRequestId,
            new DateTimeOffset(638242288542961190, TimeSpan.Zero),
            new DateTimeOffset(638242288542961190, TimeSpan.Zero),
            TimeSpan.FromHours(8),
            Guid.NewGuid(),
            "fakeRemarks",
            FederatedUser.Create("2", "filip@fake.com", "Filip")
            );
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(createdEvent);
        repositoryMock
            .Setup(s => s.FindById(command.LeaveRequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        var handleAcceptLeaveRequest = new HandleBasicAcceptLeaveRequest(repositoryMock.Object);
        //When
        await handleAcceptLeaveRequest.Handle(command, CancellationToken.None);
        //Then
        leaveRequest.Should().BeEquivalentTo(new
        {
            Status = LeaveRequestStatus.Accepted,
            LastModifiedBy = command.AcceptedBy,
            Remarks = new[]
            {
                new LeaveRequest.RemarksModel(createdEvent.Remarks, createdEvent.CreatedBy),
                new LeaveRequest.RemarksModel(command.Remarks, command.AcceptedBy)
            },
            Version = 2
        }, o => o.ExcludingMissingMembers());
        repositoryMock.Verify(x => x.Update(leaveRequest, null, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }
}