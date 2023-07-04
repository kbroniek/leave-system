using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using FluentAssertions;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.AcceptingLeaveRequest;
using LeaveSystem.EventSourcing.LeaveRequests.CreatingLeaveRequest;
using LeaveSystem.Services;
using LeaveSystem.Shared;
using LeaveSystem.Shared.LeaveRequests;
using Marten.Internal.Sessions;
using Moq;
using Xunit;
using LeaveRequest = LeaveSystem.EventSourcing.LeaveRequests.LeaveRequest;

namespace LeaveSystem.UnitTests;

public class AcceptLeaveRequestTest
{
    private IRepository<LeaveRequest>? repository;
    private LeaveRequestFactory leaveRequestFactory;
    [Fact]
    public async Task
        GivenAcceptLeaveRequestSetup_WhenAcceptLeaveRequestHandled_ThenThrowNotFoundException()
    {
        //Given
        repository = new InMemoryRepository<LeaveRequest>();
        var federatedUser = FederatedUser.Create("1","testEmail@test.com","John");
        var command = AcceptLeaveRequest.Create(Guid.NewGuid(), "remarks", federatedUser);
        var handleAcceptLeaveRequest = new HandleAcceptLeaveRequest(repository);
        //When
        Func<Task> act = async () =>
        {
            await handleAcceptLeaveRequest.Handle(command, CancellationToken.None);
        };
        //Then
        await act.Should().ThrowAsync<GoldenEye.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task
        GivenAcceptLeaveRequestSetup_WhenAcceptLeaveRequestHandled_ThenUpdate()
    {
        //Given
        repository = new InMemoryRepository<LeaveRequest>();
        
        var leaveRequest = new LeaveRequest(Guid.NewGuid(), LeaveRequestStatus.Pending);
        await repository.Add(leaveRequest);
        var federatedUser = FederatedUser.Create("1","testEmail@test.com","John");
        var command = AcceptLeaveRequest.Create(leaveRequest.Id, "remarks", federatedUser);
        var handleAcceptLeaveRequest = new HandleAcceptLeaveRequest(repository);
        //When
        await handleAcceptLeaveRequest.Handle(command, CancellationToken.None);
        var updatedLeaveRequest =  await repository.FindById(command.LeaveRequestId);
        //Then
        updatedLeaveRequest.Should()
            .Match<LeaveRequest>(x =>
                x.Id == command.LeaveRequestId &&
                x.LastModifiedBy == federatedUser &&
                x.Remarks.Last().Remarks == command.Remarks
            );
        }
}