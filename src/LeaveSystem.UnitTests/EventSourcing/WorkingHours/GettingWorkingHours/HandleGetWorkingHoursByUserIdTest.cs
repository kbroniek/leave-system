using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Exceptions;
using LeaveSystem.EventSourcing.WorkingHours.GettingWorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using NSubstitute;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.GettingWorkingHours;

public class HandleGetWorkingHoursByUserIdTest
{
    private IDocumentSession querySessionMock;

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
        querySessionMock = Substitute.For<IDocumentSession>();
        querySessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryableStub);
        var request = GetWorkingHoursByUserId.Create(FakeUserProvider.BenId);
        var sut = GetSut();
        //When
        var act = async () => { await sut.Handle(request, CancellationToken.None); };
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
        querySessionMock = Substitute.For<IDocumentSession>();
        querySessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryableStub);
        var request = GetWorkingHoursByUserId.Create(FakeUserProvider.BenId);
        var sut = GetSut();
        //When
        var result = await sut.Handle(request, CancellationToken.None);
        //Then
        result.Should().BeEquivalentTo(fakeWorkingHours.First(w => w.UserId == FakeUserProvider.BenId));
    }
}