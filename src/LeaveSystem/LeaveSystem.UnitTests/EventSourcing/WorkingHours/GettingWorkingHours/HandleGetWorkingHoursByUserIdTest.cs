using FluentAssertions;
using GoldenEye.Backend.Core.Exceptions;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using NSubstitute;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.GettingWorkingHours;

public class HandleGetWorkingHoursByUserIdTest
{
    private readonly IDocumentSession querySessionMock = Substitute.For<IDocumentSession>();

    private HandleGetWorkingHoursByUserId GetSut() => new(querySessionMock);

    [Fact]
    public async Task WhenThereIsNoWorkingHoursForGivenUser_ThrowNotFoundException()
    {
        //
        var workingHours = FakeWorkingHoursCreatedProvider.GetAll(false)
            .Select(LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours);
        var martenQueryableStub = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            workingHours
        //Todo: Add deprecated WorkingHours
        );
        querySessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryableStub);
        var request = GetCurrentWorkingHoursByUserId.Create(FakeUserProvider.BenId);
        var sut = GetSut();
        //When
        var act = () => sut.Handle(request, CancellationToken.None);
        //Then
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task WhenThereIsWorkingHoursForGivenUser_ThenReturnIt()
    {
        //Given
        var fakeWorkingHours = FakeWorkingHoursCreatedProvider.GetAll()
            .Select(LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours).ToArray();
        var martenQueryableStub = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            fakeWorkingHours
        //Todo: Add deprecated WorkingHours
        );
        querySessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryableStub);
        var request = GetCurrentWorkingHoursByUserId.Create(FakeUserProvider.BenId);
        var sut = GetSut();
        //When
        var result = await sut.Handle(request, CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(fakeWorkingHours.First(w => w.UserId == FakeUserProvider.BenId));
    }
}
