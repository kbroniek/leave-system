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
        var repositoryMock = new Mock<IRepository<LeaveRequest>>();
        var request = GetLeaveRequestDetails.Create(Guid.Parse("c74e0e31-a3fb-4227-93e4-9cfea01e7eca"));
        var sut = new HandleGetLeaveRequestDetails(repositoryMock.Object);
        //When
        var act = async () => await sut.Handle(request, CancellationToken.None);
        //Then
        await act.Should()
            .ThrowAsync<GoldenEye.Backend.Core.Exceptions.NotFoundException>()
            .WithMessage("LeaveRequest with id: c74e0e31-a3fb-4227-93e4-9cfea01e7eca was not found.");

    }

    [Fact]
    public async Task WhenLeaveRequestExists_ThenReturnIt()
    {
        //Given
        var repositoryMock = new Mock<IRepository<LeaveRequest>>();
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(
            FakeLeaveRequestCreatedProvider.GetSickLeaveRequest()
        );
        repositoryMock
            .Setup(x => x.FindByIdAsync(leaveRequest.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaveRequest);
        var request = GetLeaveRequestDetails.Create(leaveRequest.Id);
        var sut = new HandleGetLeaveRequestDetails(repositoryMock.Object);
        //When
        var result = await sut.Handle(request, CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(leaveRequest);
    }
}
