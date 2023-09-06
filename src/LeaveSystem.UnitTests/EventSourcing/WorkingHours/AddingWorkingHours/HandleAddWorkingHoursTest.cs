using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoldenEye.Repositories;
using LeaveSystem.EventSourcing.WorkingHours.AddingWorkingHours;
using LeaveSystem.Shared.Extensions;
using LeaveSystem.Shared.WorkingHours;
using LeaveSystem.UnitTests.Providers;
using LeaveSystem.UnitTests.Stubs;
using Marten;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace LeaveSystem.UnitTests.EventSourcing.WorkingHours.AddingWorkingHours;

public class HandleAddWorkingHoursTest
{
    private IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> repositoryMock;
    private WorkingHoursFactory factoryMock;
    private IDocumentSession documentSessionMock;

    private HandleAddWorkingHours GetSut() => new(repositoryMock, factoryMock, documentSessionMock);

    [Fact]
    public async Task WhenAddThrowException_ThenNotSaveChanges()
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        factoryMock = Substitute.For<WorkingHoursFactory>();
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration));
        factoryMock.Create(command).Returns(
            fakeWorkingHours
        );
        repositoryMock = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        repositoryMock.Add(fakeWorkingHours, Arg.Any<CancellationToken>()).Throws<Exception>();
        var sut = GetSut();
        //When
        var act = async () => { await sut.Handle(command, CancellationToken.None); };
        //Then
        await act.Should().ThrowAsync<Exception>();
        factoryMock.Received(1).Create(ArgExtensions.IsEquivalentTo<AddWorkingHours>(command));
        await repositoryMock.Received(1).Add(fakeWorkingHours, Arg.Any<CancellationToken>());
        await repositoryMock.DidNotReceiveWithAnyArgs().SaveChanges();
    }

    [Theory]
    [MemberData(nameof(Get_WhenHandlingSuccessful_PassAllMethodsDeprecateOldWorkingHoursForUserIfExistsAndReturnUnitValue_TestData))]
    public async Task WhenHandlingSuccessful_PassAllMethodsDeprecateOldWorkingHoursForUserIfExistsAndReturnUnitValue(IEnumerable<LeaveSystem.EventSourcing.WorkingHours.WorkingHours> workingHoursEnumerable)
    {
        //Given
        var command = FakeCreateWorkingHoursProvider.GetForBen();
        factoryMock = Substitute.For<WorkingHoursFactory>();
        var fakeWorkingHours = LeaveSystem.EventSourcing.WorkingHours.WorkingHours.CreateWorkingHours(
            WorkingHoursCreated.Create(command.WorkingHoursId, command.UserId, command.DateFrom,
                command.DateTo, command.Duration));
        factoryMock.Create(command).Returns(
            fakeWorkingHours
        );
        repositoryMock = Substitute.For<IRepository<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>>();
        documentSessionMock = Substitute.For<IDocumentSession>();
        var martenQueryable = new MartenQueryableStub<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>(
            workingHoursEnumerable.ToList()
        );
        documentSessionMock.Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>()
            .Returns(martenQueryable);
        var sut = GetSut();
        //When
        var result = await sut.Handle(command, CancellationToken.None);
        //Then
        documentSessionMock.Received(1).Query<LeaveSystem.EventSourcing.WorkingHours.WorkingHours>();
        factoryMock.Received(1).Create(ArgExtensions.IsEquivalentTo<AddWorkingHours>(command));
        await repositoryMock.Received(1).Add(fakeWorkingHours, Arg.Any<CancellationToken>());
        await repositoryMock.Received(1).SaveChanges();
        result.Should().BeEquivalentTo(Unit.Value);
        martenQueryable.Any(x => x.UserId == command.UserId && x.Status == WorkingHoursStatus.Current)
            .Should().BeFalse();
    }

    public static IEnumerable<object[]> Get_WhenHandlingSuccessful_PassAllMethodsDeprecateOldWorkingHoursForUserIfExistsAndReturnUnitValue_TestData()
    {
        yield return new object[] { FakeWorkingHoursProvider.GetAll() };
        yield return new object[] { FakeWorkingHoursProvider.GetCurrent() };
        yield return new object[] { FakeWorkingHoursProvider.GetDeprecated() };
    }
}
