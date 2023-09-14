using System;
using System.Threading;
using System.Threading.Tasks;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Shared;
using LeaveSystem.Shared.WorkingHours;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.CreatingLeaveRequest;

public class HandleCreateLeaveRequestOnBehalfTest
{
    private readonly Mock<IRepository<LeaveRequest>> repositoryMock = new();
    private readonly Mock<LeaveRequestFactory> factoryMock = new(null, null, null);
    private readonly CreateLeaveRequest command = CreateLeaveRequest.Create(
        Guid.NewGuid(),
        new DateTimeOffset(638242288542961190, TimeSpan.Zero),
        new DateTimeOffset(638242288542961190, TimeSpan.Zero),
        TimeSpan.FromHours(8),
        Guid.NewGuid(),
        "fake remarks",
        FederatedUser.Create("fakeId", "fake@emial.com", "Philip")
    );
    
    [Fact]
    public async Task WhenCreateLeaveRequestHandled_ThenCreate()
    {
        //Given
        var createdEvent = LeaveRequestCreated.Create(
            command.LeaveRequestId,
            command.DateFrom,
            command.DateTo,
            command.Duration!.Value,
            command.LeaveTypeId,
            command.Remarks,
            command.CreatedBy,
            WorkingHoursUtils.DefaultWorkingHours
        );
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(createdEvent);
        factoryMock
            .Setup(f => f.Create(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        var handleAcceptLeaveRequest = new HandleCreateLeaveRequest(repositoryMock.Object, factoryMock.Object);
        //When
        await handleAcceptLeaveRequest.Handle(command, CancellationToken.None);
        //Then
        factoryMock.Verify(x => 
            x.Create(command, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => 
            x.Add(leaveRequest, It.IsAny<CancellationToken>()));
        repositoryMock.Verify(x => 
            x.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
    }
}