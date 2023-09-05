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
        //Given
        var martenQueryableStub = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            FakeWorkingHoursCreatedProvider.GetAll(false)
                .Select(LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours)
                //Todo: Add deprecated WorkingHours
                );
        querySessionMock = Substitute.For<IDocumentSession>();
        querySessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryableStub);
        var request = GetWorkingHoursByUserId.Create(FakeUserProvider.BenId);
        var sut = GetSut();
        //When
        var act = async () =>
        {
            await sut.Handle(request, CancellationToken.None);
        };
        //Then
        await act.Should().ThrowAsync<NotFoundException>();
    }
}