using FluentAssertions;
using GoldenEye.Backend.Core.Repositories;
using LeaveSystem.EventSourcing.LeaveRequests;
using LeaveSystem.EventSourcing.LeaveRequests.RejectingLeaveRequest;
using LeaveSystem.UnitTests.Providers;
using Moq;
using Xunit;
using Unit = MediatR.Unit;

namespace LeaveSystem.UnitTests.EventSourcing.LeaveRequests.RejectingLeaveReuest;

public class HandleRejectLeaveRequestTest
{
    [Fact]
    public async Task WhenLeaveRequestWithThisIdNotFoundInRepository_ThenThrowNotFoundException()
    {
        //Given
        var command = RejectLeaveRequest.Create(Guid.NewGuid(), "fake remarks", FakeUserProvider.GetUserWithNameFakeoslav());
        var repositoryMock = new Mock<IRepository<LeaveRequest>>();
        repositoryMock.Setup(x => x.FindByIdAsync(command.LeaveRequestId, It.IsAny<CancellationToken>()))!
            .ReturnsAsync((LeaveRequest?)null);
        var sut = new HandleRejectLeaveRequest(repositoryMock.Object);
        //When
        var act = async () => await sut.Handle(command, It.IsAny<CancellationToken>());
        //Then 
        await act.Should().ThrowAsync<GoldenEye.Backend.Core.Exceptions.NotFoundException>();
        repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenNoExceptions_ThenReturnUnit()
    {
        //Given
        var leaveRequest = LeaveRequest.CreatePendingLeaveRequest(
            FakeLeaveRequestCreatedProvider.GetSickLeaveRequest()
        );
        var command = RejectLeaveRequest.Create(leaveRequest.Id, "fake remarks", FakeUserProvider.GetUserWithNameFakeoslav());
        var repositoryMock = new Mock<IRepository<LeaveRequest>>();
        repositoryMock.Setup(x => x.FindByIdAsync(command.LeaveRequestId, It.IsAny<CancellationToken>()))!
            .ReturnsAsync(leaveRequest);
        var sut = new HandleRejectLeaveRequest(repositoryMock.Object);
        //When
        var result = await sut.Handle(command, It.IsAny<CancellationToken>());
        result.Should().BeEquivalentTo(Unit.Value);
        //Then 
        repositoryMock.Verify(r => r.UpdateAsync(leaveRequest, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
