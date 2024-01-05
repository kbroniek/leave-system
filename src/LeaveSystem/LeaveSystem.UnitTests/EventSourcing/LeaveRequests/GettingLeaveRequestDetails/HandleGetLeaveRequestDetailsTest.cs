using FluentAssertions;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.GettingLeaveRequestDetails;
using LeaveSystem.UnitTests.Providers;
using Moq;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.GettingLeaveRequestDetails;

public class HandleGetLeaveRequestDetailsTest
{
    [Fact]
    public async Task WhenLeaveRequestNotExists_ThenThrowNotFoundException()
    {
        //Given
        var repository = new InMemoryRepository<LeaveRequest>();
        var request = GetLeaveRequestDetails.Create(Guid.NewGuid());
        var sut = new HandleGetLeaveRequestDetails(repository);
        //When
        var act = async () => await sut.Handle(request, It.IsAny<CancellationToken>());
        //Then
        await act.Should().ThrowAsync<GoldenEye.Backend.Core.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task WhenLeaveRequestExists_ThenReturnIt()
    {
        //Given
        var repository = new InMemoryRepository<LeaveRequest>();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(
            FakeLeaveRequestCreatedProvider.GetSickLeaveRequest()
        );
        await repository.AddAsync(leaveRequest, It.IsAny<CancellationToken>());
        var request = GetLeaveRequestDetails.Create(leaveRequest.Id);
        var sut = new HandleGetLeaveRequestDetails(repository);
        //When
        var result = await sut.Handle(request, It.IsAny<CancellationToken>());
        //Then
        result.Should().BeEquivalentTo(leaveRequest);
    }
}
